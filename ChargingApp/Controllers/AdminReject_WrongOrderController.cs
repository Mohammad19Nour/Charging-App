using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Helpers;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

[Authorize(Policy = "Required_AnyAdmin_Role")]
public class AdminRejectWrongOrderController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly UserManager<AppUser> _userManager;

    public AdminRejectWrongOrderController(IUnitOfWork unitOfWork, INotificationService notificationService,
        UserManager<AppUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _userManager = userManager;
    }
    
    [HttpGet("reject-wrong/{orderId:int}")]
    public async Task<ActionResult> RejectOrWrong(int orderId, string type, string notes = "")
    {
        //type either wrong or reject

        try
        {
            type = type.ToLower();

            if (type != "wrong" && type != "reject")
                return BadRequest(new ApiResponse(400, "type should be wrong or reject"));

            var order = await _unitOfWork.OrdersRepository.GetOrderByIdAsync(orderId);

            if (order is null) return NotFound(new ApiResponse(401, "this order doesn't exist"));

            if (order.Status == 0)
                return BadRequest(new ApiResponse(400, "Please receive this order before approve it"));

            if (order.Status != 4)
                return BadRequest(new ApiResponse(400, "this order already checked"));

            if (order.OrderType.ToLower() == "vip" && type == "reject")
                return BadRequest(new ApiResponse(400, "can't reject this order"));

            order.Status = type == "reject" ? 2 : 3;

            order.Notes = notes;

            var user = order.User;
            var roles = await _userManager.GetRolesAsync(user);

            var vip = roles.Any(x => x.ToLower() == "vip");
            if (vip)
            {
                var lastVip = order.User.VIPLevel;
                user.Balance += order.TotalPrice;
                user.TotalPurchasing -= order.TotalPrice;
                user.TotalForVIPLevel -= order.TotalPrice;
                user.VIPLevel = await _unitOfWork.VipLevelRepository
                    .GetVipLevelForPurchasingAsync(user.TotalForVIPLevel);
                _unitOfWork.UserRepository.UpdateUserInfo(user);

                if (lastVip > order.User.VIPLevel)
                {
                    var curr = new NotificationHistory
                    {
                        User = order.User,
                        ArabicDetails = " تم اعادة مستواك الى vip  " + order.User.VIPLevel,
                        EnglishDetails = "Your level is returned back to vip " + order.User.VIPLevel
                    };
                    _unitOfWork.NotificationRepository.AddNotificationForHistoryAsync(curr);

                    await _notificationService.VipLevelNotification(order.User.Email,
                        "Vip level status notification", SomeUsefulFunction.GetOrderNotificationDetails(order));
                }
            }

            var cur = new NotificationHistory
            {
                User = order.User,
                ArabicDetails = order.Notes+" : "+ "تم رفض الطلب رقم " + orderId + " لأن " ,
                EnglishDetails = "Order with id " + orderId + " has been rejected by admin because: " + order.Notes
            };

            _unitOfWork.NotificationRepository.AddNotificationForHistoryAsync(cur);

            var not = new OrderAndPaymentNotification
            {
                User = order.User,
                Order = order
            };
            await _notificationService.NotifyUserByEmail(order.User.Email, _unitOfWork, not,
                "Order status notification", SomeUsefulFunction.GetOrderNotificationDetails(order));

            if (!await _unitOfWork.Complete()) return BadRequest(new ApiResponse(400, "Failed to reject order"));

            return Ok(new ApiResponse(200, "Done successfully"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}