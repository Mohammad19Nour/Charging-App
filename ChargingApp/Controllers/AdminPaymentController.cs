﻿using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Helpers;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChargingApp.Controllers;

[Authorize(Policy = "Required_AnyAdmin_Role")]
public class AdminPaymentController : AdminController
{
    private static readonly SemaphoreSlim Semaphore = new(1, 1);
    private static readonly SemaphoreSlim Semaphore1 = new(1, 1);
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public AdminPaymentController(IUnitOfWork unitOfWork, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    [HttpGet("approve/{paymentId:int}")]
    public async Task<ActionResult> ApprovePayment(int paymentId)
    {
        try
        {
            await Semaphore.WaitAsync();

            var payment = await _unitOfWork.PaymentRepository.GetPaymentByIdAsync(paymentId);

            if (payment is null)
            {
                Semaphore.Release();
                return NotFound(new ApiResponse(401, "this payment doesn't exist"));
            }

            if (payment.Status != 0)
            {
                Semaphore.Release();
                return BadRequest(new ApiResponse(400, "this payment already checked"));
            }

            payment.Status = 1;

            var mn = Math.Min(payment.AddedValue, payment.User.Debit);

            payment.User.Debit -= mn;
            payment.AddedValue -= mn;

            payment.User.Balance += payment.AddedValue;
            payment.AddedValue += mn;

            _unitOfWork.UserRepository.UpdateUserInfo(payment.User);
            _unitOfWork.NotificationRepository.AddNotificationForHistoryAsync(
                new NotificationHistory
                {
                    User = payment.User,
                    ArabicDetails = " تم قبول  الدفعة رقم " + paymentId + " من قبل الادمن ",
                    EnglishDetails = "Payment with id " + paymentId + " has been approved by admin"
                });
            var not = new OrderAndPaymentNotification
            {
                Payment = payment,
                User = payment.User
            };
            await _notificationService.NotifyUserByEmail(payment.User.Email, _unitOfWork, not,
                "Payment status notification", SomeUsefulFunction.GetPaymentNotificationDetails(payment));

            if (await _unitOfWork.Complete())
            {
                Semaphore.Release();
                return Ok(new ApiResponse(200, "approved successfully"));
            }

            return BadRequest(new ApiResponse(400, "Failed to approve payment"));
        }
        catch (Exception e)
        {
            Semaphore.Release();
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpGet("reject/{paymentId:int}")]
    public async Task<ActionResult> RejectPayment(int paymentId, string notes = "")
    {
        try
        {
            await Semaphore1.WaitAsync();
            var payment = await _unitOfWork.PaymentRepository.GetPaymentByIdAsync(paymentId);

            if (payment is null)
            {
                Semaphore1.Release();
                return NotFound(new ApiResponse(401, "this payment doesn't exist"));
            }

            if (payment.Status != 0)
            {
                Semaphore1.Release();
                return BadRequest(new ApiResponse(400, "this payment already checked"));
            }

            payment.Status = 2;
            payment.Notes = notes;


            var not = new OrderAndPaymentNotification
            {
                Payment = payment,
                User = payment.User
            };

            await _notificationService.NotifyUserByEmail(payment.User.Email, _unitOfWork, not,
                "Payment status notification", SomeUsefulFunction.GetPaymentNotificationDetails(payment));

            _unitOfWork.NotificationRepository.AddNotificationForHistoryAsync(
                new NotificationHistory
                {
                    User = payment.User,
                    ArabicDetails = payment.Notes+" : "+ "تم رفض الدفعة رقم " + paymentId + " لأن " ,
                    EnglishDetails = "Payment with id " + paymentId + " has been rejected by admin because: " +
                                     payment.Notes
                });
            if (await _unitOfWork.Complete())
            {
                Semaphore1.Release();
                return Ok(new ApiResponse(200, "Rejected successfully"));
            }

            Semaphore1.Release();
            return BadRequest(new ApiResponse(400, "Failed to reject order"));
        }
        catch (Exception e)
        {
            Semaphore1.Release();
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpGet("pending-payment")]
    public async Task<ActionResult<List<PaymentAdminDto>>> GetPendingPayments(string? user = null)
    {
        try
        {
            var res = await _unitOfWork.PaymentRepository.GetAllPendingPaymentsAsync(user);
            return Ok(new ApiOkResponse<List<PaymentAdminDto>>(res));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }


    // delete done orders in last 6 months
    [HttpDelete("delete-orders")]
    public async Task<ActionResult<bool>> DeleteOrders()
    {
        try
        {
            var dateTime = DateTime.UtcNow.AddDays(-183);

            var paymentQuery = _unitOfWork.PaymentRepository.GetQueryable();
            paymentQuery = paymentQuery.Where(x => x.Status == 1)
                .Where(x => x.CreatedDate < dateTime);
            var payments = await paymentQuery.ToArrayAsync();

            foreach (var payment in payments)
            {
                var notList = await _unitOfWork.NotificationRepository
                    .GetOrdersNotifications(payment.Id);
                foreach (var not in notList)
                    _unitOfWork.NotificationRepository.DeleteNotification(not);
            }

            _unitOfWork.PaymentRepository.DeletePayments(payments);

            if (await _unitOfWork.Complete())
                return Ok(new ApiResponse(200, "All payments Before six months has been deleted successfully"));

            if (!_unitOfWork.HasChanges())
                return Ok(new ApiResponse(200, "There are no payments before six months to be deleted"));

            return BadRequest(new ApiResponse(400, "Failed to delete payments"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}