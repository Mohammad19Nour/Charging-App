using AutoMapper;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Helpers;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

[Authorize(Policy = "Required_AnyAdmin_Role")]
public class AdminOrderController : AdminController
{
    private readonly List<string> _status = new()
        { "Pending", "Succeed", "Rejected", "Wrong", "Received", "Cancelled" };

    private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);
    private static readonly SemaphoreSlim Semaphore1 = new(1, 1);
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;
    private readonly IApiService _apiService;
    private readonly UserManager<AppUser> _userManager;

    public AdminOrderController(IUnitOfWork unitOfWork, INotificationService notificationService
        , IMapper mapper, IApiService apiService, UserManager<AppUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _mapper = mapper;
        _apiService = apiService;
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
                    .SendOrderAsync(apiId, order.TotalQuantity, order.PlayerId,hostingSite.BaseUrl
                    ,hostingSite.Token);

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
                        order.TotalPrice = await SomeUsefulFunction.CalcTotalPriceCannotChooseQuantity
                            (order.TotalQuantity, order.Product, order.User, _unitOfWork);
                }
                else
                {
                    if (order.Product != null)
                        order.TotalQuantity = await
                            SomeUsefulFunction.CalcTotalQuantity(order.Product.Quantity, order.Product
                                , order.User, _unitOfWork);
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
                        ArabicDetails = " تم ترقي مستواك الى vip  " + order.User.VIPLevel,
                        EnglishDetails = "Your level is upgrade to vip " + order.User.VIPLevel
                    };
                   _unitOfWork.NotificationRepository.AddNotificationForHistoryAsync(curr);

                    await _notificationService.VipLevelNotification(order.User.Email,
                        "Vip level status notification", SomeUsefulFunction.GetOrderNotificationDetails(order));
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

            if (order.OrderType.ToLower() == "vip" && type == "rejected")
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
                
                if (lastVip > order.User.VIPLevel)
                {
                    var curr = new NotificationHistory
                    {
                        User = order.User,
                        ArabicDetails = " تم اعادة مستواك الى  " + order.User.VIPLevel,
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
                ArabicDetails = "تم رفض الطلب رقم " + orderId + " لانه خاطئ ",
                EnglishDetails = "Order with id " + orderId + " has been rejected by admin"
            };

            if (order.Status != 2)
            {
                cur.ArabicDetails = "تم رفض الطلب رقم " + orderId + " لانه خاطئ ";
                cur.EnglishDetails = "Order with id " + orderId + " has been rejected by admin because it's wrong";
            }

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

    [HttpGet("pending-orders")]
    public async Task<ActionResult<List<PendingOrderDto>>> GetPendingOrders()
    {
        try
        {
            var res = await _unitOfWork.OrdersRepository.GetPendingOrdersAsync();
            var tmp = new List<PendingOrderDto>();

            foreach (var t in res)
            {
                if (t.OrderType.ToLower() == "normal")
                {
                    tmp.Add(_mapper.Map<PendingOrderDto>(t));
                }
                else
                {
                    t.User.TotalPurchasing -= t.TotalPrice;
                    t.User.TotalForVIPLevel -= t.TotalPrice;
                    t.User.Balance += t.TotalPrice;
                    t.User.VIPLevel = await _unitOfWork.VipLevelRepository
                        .GetVipLevelForPurchasingAsync(t.User.TotalForVIPLevel);

                    if (t.Product is null) continue;

                    if (!t.CanChooseQuantity)
                    {
                        var pr = await SomeUsefulFunction.CalcTotalPriceCannotChooseQuantity
                            (t.TotalQuantity, t.Product, t.User, _unitOfWork);
                        t.TotalPrice = pr;
                    }
                    else
                    {
                        t.TotalQuantity = await
                            SomeUsefulFunction.CalcTotalQuantity(t.Product.Quantity, t.Product
                                , t.User, _unitOfWork);
                    }

                    if (t.User.Balance >= t.TotalPrice)
                    {
                        t.User.TotalPurchasing += t.TotalPrice;
                        t.User.TotalForVIPLevel += t.TotalPrice;
                        t.User.Balance -= t.TotalPrice;
                        t.User.VIPLevel = await _unitOfWork.VipLevelRepository
                            .GetVipLevelForPurchasingAsync(t.User.TotalForVIPLevel);
                    }

                    var x = new PendingOrderDto();
                    _mapper.Map(t, x);
                    tmp.Add(x);
                }
            }

            tmp = tmp.OrderByDescending(x => x.CreatedAt).ToList();
            return Ok(new ApiOkResponse(tmp));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
            // BadRequest(new ApiException(500,e.Message,e.StackTrace));
        }
    }

    [HttpGet("pending-orders/{email}")]
    public async Task<ActionResult<List<PendingOrderDto>>> GetPendingOrders(string email)
    {
        try
        {
            var res = await _unitOfWork.OrdersRepository.GetPendingOrdersAsync(email);

            var tmp = new List<PendingOrderDto>();

            foreach (var t in res)
            {
                if (t.OrderType.ToLower() == "normal")
                {
                    tmp.Add(_mapper.Map<PendingOrderDto>(t));
                }
                else
                {
                    if (t.Product is null) continue;

                    t.User.TotalPurchasing -= t.TotalPrice;
                    t.User.TotalForVIPLevel -= t.TotalPrice;
                    t.User.Balance += t.TotalPrice;
                    t.User.VIPLevel = await _unitOfWork.VipLevelRepository
                        .GetVipLevelForPurchasingAsync(t.User.TotalForVIPLevel);

                    if (!t.CanChooseQuantity)
                    {
                        var pr = await SomeUsefulFunction.CalcTotalPriceCannotChooseQuantity
                            (t.TotalQuantity, t.Product, t.User, _unitOfWork);
                        t.TotalPrice = pr;
                    }
                    else
                    {
                        t.TotalQuantity = await
                            SomeUsefulFunction.CalcTotalQuantity(t.Product.Quantity, t.Product
                                , t.User, _unitOfWork);
                    }

                    if (t.User.Balance >= t.TotalPrice)
                    {
                        t.User.TotalPurchasing += t.TotalPrice;
                        t.User.TotalForVIPLevel += t.TotalPrice;
                        t.User.Balance -= t.TotalPrice;
                        t.User.VIPLevel = await _unitOfWork.VipLevelRepository
                            .GetVipLevelForPurchasingAsync(t.User.TotalForVIPLevel);
                    }

                    var x = new PendingOrderDto();
                    _mapper.Map(t, x);
                    tmp.Add(x);
                }
            }

            tmp = tmp.OrderByDescending(x => x.CreatedAt).ToList();
            return Ok(new ApiOkResponse(tmp));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}