using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Helpers;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

[Authorize(Policy = "Required_AnyAdmin_Role")]
public class AdminApproveOrderController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IApiService _apiService;
    private readonly UserManager<AppUser> _userManager;
    private static readonly SemaphoreSlim Semaphore1 = new(1, 1);

    public AdminApproveOrderController(IUnitOfWork unitOfWork, INotificationService notificationService,
        IApiService apiService, UserManager<AppUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _apiService = apiService;
        _userManager = userManager;
    }

    [HttpGet("approve/{orderId:int}")]
    public async Task<ActionResult> Approve(int orderId)
    {
        try
        {
            await Semaphore1.WaitAsync();
            var order = await _unitOfWork.OrdersRepository.GetOrderByIdAsync(orderId);

            if (order is null)
            {
                Semaphore1.Release();
                return NotFound(new ApiResponse(401, "this order doesn't exist"));
            }

            if (order.Status == 0)
            {
                Semaphore1.Release();
                return BadRequest(new ApiResponse(400, "Please receive this order before approve it"));
            }

            if (order.Status != 4)
            {
                Semaphore1.Release();
                return BadRequest(new ApiResponse(400, "this order already checked"));
            }

            if (order.Product == null)
                return BadRequest(new ApiResponse(400
                    , "The product not found.. it maybe deleted"));

            var tmp = await _unitOfWork.OtherApiRepository
                .CheckIfProductExistAsync(order.Product.Id, true); //from other site

            if (tmp)
            {
                var roles = await _userManager.GetRolesAsync(order.User);
                if (roles.Any(x => x.ToLower() == "vip"))
                {
                    Semaphore1.Release();
                    return BadRequest(new ApiResponse(400, "this order from another site"));
                }

                var apiId = await _unitOfWork.OtherApiRepository
                    .GetApiProductIdAsync(order.Product.Id);

                var hostingSite = (await _unitOfWork.OtherApiRepository
                    .GetOrderByOurIdAsync(orderId)).HostingSite;

                var response = await _apiService
                    .SendOrderAsync(apiId, order.TotalQuantity, order.PlayerId, hostingSite.BaseUrl
                        , hostingSite.Token);

                if (!response.Success)
                {
                    Semaphore1.Release();
                    return BadRequest(new ApiResponse(400, response.Message));
                }

                var api = new ApiOrder
                {
                    Order = order,
                    ApiOrderId = response.OrderId
                };
                _unitOfWork.OtherApiRepository.AddOrder(api);
                var not = new OrderAndPaymentNotification
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
                        ArabicDetails = " تم قبول الطلب رقم " + orderId,
                        EnglishDetails = "Order with id " + orderId + " has been approved by admin"
                    });

                if (await _unitOfWork.Complete())
                {
                    Semaphore1.Release();
                    return Ok(new ApiResponse(200,
                        order.Status != 3 ? "approved successfully" : "the user have no enough money"));
                }

                Semaphore1.Release();
                return BadRequest(new ApiResponse(400, "Failed to approve order"));
            }

            order.Status = 1; // accepted

            if (order.OrderType.ToLower() == "vip")
            {
                order.User.TotalPurchasing -= order.TotalPrice;
                order.User.TotalForVIPLevel -= order.TotalPrice;
                order.User.Balance += order.TotalPrice;
                order.User.VIPLevel = await _unitOfWork.VipLevelRepository
                    .GetVipLevelForPurchasingAsync(order.User.TotalForVIPLevel);

                var lastVip = order.User.VIPLevel;

                if (!order.CanChooseQuantity)
                {
                    if (order.Product != null)
                        order.TotalPrice = await PriceForVIP.CannotChooseQuantity
                            (order.TotalQuantity, order.Product, order.User, _unitOfWork);
                }
                else
                {
                    if (order.Product != null)
                        order.TotalQuantity = await
                            PriceForVIP.CalcTotalQuantity(order.Product.Quantity, order.Product
                                , order.User, _unitOfWork);
                    var specificPrice = await _unitOfWork.SpecificPriceForUserRepository
                        .GetProductPriceForUserAsync(order.Product.Id, order.User);

                    order.Quantity = order.Product.Quantity;
                    if (specificPrice != null)
                    {
                        order.TotalPrice = (decimal)specificPrice;
                        order.TotalQuantity = order.Quantity;
                    }
                    else
                        order.TotalPrice = order.Product.Price;

                    order.User.TotalPurchasing += order.TotalPrice;
                    order.User.TotalForVIPLevel += order.TotalPrice;
                }

                if (order.TotalPrice <= order.User.Balance)
                {
                    order.User.VIPLevel = await _unitOfWork.VipLevelRepository
                        .GetVipLevelForPurchasingAsync(order.User.TotalForVIPLevel);
                    order.User.Balance -= order.TotalPrice;
                    order.Status = 1; // accepted

                    order.Notes = "Succeed";
                }
                else
                {
                    order.User.TotalPurchasing -= order.TotalPrice;
                    order.User.TotalForVIPLevel -= order.TotalPrice;
                    order.User.VIPLevel = await _unitOfWork.VipLevelRepository
                        .GetVipLevelForPurchasingAsync(order.User.TotalForVIPLevel);
                    order.Status = 3; // wrong
                    order.Notes = "No enough money";
                }

                if (lastVip < order.User.VIPLevel)
                { 
                    var curr = new NotificationHistory
                    {
                        User = order.User,
                        ArabicDetails = " تم ترقية مستواك الى vip  " + order.User.VIPLevel,
                        EnglishDetails = "Your level has been upgraded to vip " + order.User.VIPLevel
                    };
                    _unitOfWork.NotificationRepository.AddNotificationForHistoryAsync(curr);

                    await _notificationService.VipLevelNotification(order.User.Email,
                        "Vip level status notification", 
                        SomeUsefulFunction.GetVipLevelNotification(order.User.VIPLevel));
                }
            }

            var cur = new NotificationHistory
            {
                User = order.User,
                ArabicDetails = " تم قبول الطلب رقم " + orderId,
                EnglishDetails = "Order with id " + orderId + " has been approved by admin"
            };

            if (order.Status != 1)
            {
                cur.ArabicDetails = " تم رفض الطلب رقم " + orderId;
                cur.EnglishDetails = "Order with id " + orderId + " has been rejected by admin";
            }

            _unitOfWork.NotificationRepository.AddNotificationForHistoryAsync(cur);

            var notr = new OrderAndPaymentNotification
            {
                User = order.User,
                Order = order
            };
            await _notificationService.NotifyUserByEmail(order.User.Email, _unitOfWork, notr,
                "Order status notification", SomeUsefulFunction.GetOrderNotificationDetails(order));
            _unitOfWork.UserRepository.UpdateUserInfo(order.User);

            if (await _unitOfWork.Complete())
            {
                Semaphore1.Release();
                return Ok(new ApiResponse(200,
                    order.Status != 3 ? "approved successfully" : "the user have no enough money"));
            }

            Semaphore1.Release();
            return BadRequest(new ApiResponse(400, "Failed to approve order"));
        }
        catch (Exception e)
        {
            Semaphore1.Release();
            Console.WriteLine(e);
            throw;
        }
    }
}