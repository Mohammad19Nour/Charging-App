using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Helpers;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

[Authorize(Policy = "Required_AnyAdmin_Role")]
public class AdminReceiveOrderController : BaseApiController
{
    private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly UserManager<AppUser> _userManager;

    public AdminReceiveOrderController(IUnitOfWork unitOfWork, INotificationService notificationService
        , UserManager<AppUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _userManager = userManager;
    }

    [HttpGet("receive-order")]
    public async Task<ActionResult> Receive(int orderId)
    {
        try
        {
            await Semaphore.WaitAsync();
            var order = await _unitOfWork.OrdersRepository.GetOrderByIdAsync(orderId);

            if (order is null)
            {
                Semaphore.Release();
                return NotFound(new ApiResponse(401, "this order doesn't exist"));
            }

            if (order.Status == 4)
            {
                Semaphore.Release();
                return BadRequest(new ApiResponse(400, "this order already received"));
            }

            if (order.Status != 0)
            {
                Semaphore.Release();
                return BadRequest(new ApiResponse(400, "this order already checked"));
            }

            if (order.Product == null)
            {
                Semaphore.Release();
                return BadRequest(new ApiResponse(400, "this product of order not found"));
            }

            var tmp = await _unitOfWork.OtherApiRepository
                .CheckIfProductExistAsync(order.Product.Id, true);

            if (tmp)
            {
                var res = await _unitOfWork.OtherApiRepository
                    .CheckIfOrderExistAsync(order.Id, true);
                if (res)
                {
                    Semaphore.Release();
                    return BadRequest(new ApiResponse(400, "Order already sent to another site"));
                }

                var roles = await _userManager.GetRolesAsync(order.User);

                if (roles.Any(x => x.ToLower() == "vip"))
                {
                    Semaphore.Release();
                    return BadRequest(new ApiResponse(400, "the order was sent to fast store"));
                }
            }

            order.Status = 4;

            var not = new OrderAndPaymentNotification()
            {
                User = order.User,
                Order = order
            };
            await _notificationService.NotifyUserByEmail(order.User.Email, _unitOfWork, not,
                "Order status notification", SomeUsefulFunction.GetOrderNotificationDetails(order));

            _unitOfWork.NotificationRepository.AddNotificationForHistoryAsync(
                new NotificationHistory
                {
                    User = order.User,
                    ArabicDetails = " تم استلام الطلب رقم " + orderId,
                    EnglishDetails = "Order with id " + orderId + " has been received by admin"
                });
            if (!await _unitOfWork.Complete())
            {
                Semaphore.Release();
                return BadRequest(new ApiResponse(400, "Failed to Received order"));
            }

            Semaphore.Release();
            return Ok(new ApiResponse(200, "Received successfully"));
        }
        catch (Exception e)
        {
            Semaphore.Release();
            Console.WriteLine(e);
            throw;
        }
    }
}