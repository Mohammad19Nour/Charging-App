using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using ChargingApp.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ChargingApp.Controllers;

[Authorize(Policy = "Required_AnyAdmin_Role")]
public class AdminPaymentController : AdminController
{
    List<string> status = new List<string> { "Pending", "Succeed", "Rejected", "Wrong", "Received", "Cancelled" };
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubContext<PresenceHub> _presenceHub;
    private readonly PresenceTracker _tracker;

    public AdminPaymentController(IUnitOfWork unitOfWork
        , IHubContext<PresenceHub> presenceHub, PresenceTracker tracker)
    {
        _unitOfWork = unitOfWork;
        _presenceHub = presenceHub;
        _tracker = tracker;
    }

    [HttpPost("approve/{paymentId:int}")]
    public async Task<ActionResult> ApprovePayment(int paymentId)
    {
        try
        {
            var payment = await _unitOfWork.PaymentRepository.GetPaymentByIdAsync(paymentId);

            if (payment is null) return NotFound(new ApiResponse(401, "this payment doesn't exist"));

            if (payment.Status != 0) return BadRequest(new ApiResponse(400, "this payment already checked"));

            payment.Status = 1;

            var mn = Math.Min(payment.AddedValue, payment.User.Debit);

            payment.User.Debit -= mn;
            payment.AddedValue -= mn;

            payment.User.Balance += payment.AddedValue;

            var connections = await _tracker.GetConnectionsForUser(payment.User.Email);

            if (connections != null)
            {
                await _presenceHub.Clients.Clients(connections)
                    .SendAsync("Payment status notification", getDetails(payment));
            }
            else
            {
                _unitOfWork.NotificationRepository.AddNotification(new OrderAndPaymentNotification
                {
                    User = payment.User,
                    Payment = payment
                });
            }

            if (await _unitOfWork.Complete())
            {
                return Ok(new ApiResponse(200, "approved successfully"));
            }

            return BadRequest(new ApiResponse(400, "Failed to approve payment"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpPost("reject/{paymentId:int}")]
    public async Task<ActionResult> RejectPayment(int paymentId, string notes = "")
    {
        try
        {
            var payment = await _unitOfWork.PaymentRepository.GetPaymentByIdAsync(paymentId);

            if (payment is null) return NotFound(new ApiResponse(401, "this order doesn't exist"));

            if (payment.Status != 0) return BadRequest(new ApiResponse(400, "this order already checked"));

            payment.Status = 2;
            payment.Notes = notes;

            var connections = await _tracker.GetConnectionsForUser(payment.User.Email);

            if (connections != null)
            {
                await _presenceHub.Clients.Clients(connections)
                    .SendAsync("Payment status notification", getDetails(payment));
            }
            else
            {
                _unitOfWork.NotificationRepository.AddNotification(new OrderAndPaymentNotification
                {
                    User = payment.User,
                    Payment = payment
                });
            }

            if (await _unitOfWork.Complete())
            {
                return Ok(new ApiResponse(200, "Rejected successfully"));
            }

            return BadRequest(new ApiResponse(400, "Failed to reject order"));
        }
        catch (Exception e)
        {
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
            return Ok(new ApiOkResponse(res));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    private Dictionary<string, dynamic> getDetails(Payment order)
    {
        return new Dictionary<string, dynamic>
        {
            { "paymentId", order.Id },
            { "status", "payment "+status[order.Status] }
        };
    }
}