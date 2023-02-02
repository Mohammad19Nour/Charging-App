using AutoMapper;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Helpers;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

[Authorize(Policy = "Required_AnyAdmin_Role")]
public class AdminOrderController : AdminController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;
    private readonly IApiService _apiService;

    public AdminOrderController(IUnitOfWork unitOfWork, INotificationService notificationService
        , IMapper mapper, IApiService apiService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _mapper = mapper;
        _apiService = apiService;
    }

    [HttpPost("receive-order")]
    public async Task<ActionResult> Receive(int orderId)
    {
        try
        {
            var order = await _unitOfWork.OrdersRepository.GetOrderByIdAsync(orderId);

            if (order is null) return NotFound(new ApiResponse(401, "this order doesn't exist"));

            if (order.Status == 4)
                return BadRequest(new ApiResponse(400, "this order already received"));

            if (order.Status != 0)
                return BadRequest(new ApiResponse(400, "this order already checked"));

            order.Status = 4;


            if (!await _unitOfWork.Complete()) return BadRequest(new ApiResponse(400, "Failed to Received order"));

            var not = new OrderAndPaymentNotification
            {
                User = order.User,
                Order = order
            };
            await _notificationService.NotifyUserByEmail(order.User.Email, _unitOfWork, not,
                "Order status notification", "order received by admin");

            return Ok(new ApiResponse(200, "Received successfully"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpPost("approve/{orderId:int}")]
    public async Task<ActionResult> Approve(int orderId)
    {
        try
        {
            var order = await _unitOfWork.OrdersRepository.GetOrderByIdAsync(orderId);

            if (order is null) return NotFound(new ApiResponse(401, "this order doesn't exist"));

            if (order.Status == 0)
                return BadRequest(new ApiResponse(400, "Please receive this order before approve it"));

            if (order.Status != 4)
                return BadRequest(new ApiResponse(400, "this order already checked"));

            var tmp = await _unitOfWork.OtherApiRepository
                .CheckIfProductExistAsync(order.Product.Id, true);

            if (tmp)
            {
                var res = await _unitOfWork.OtherApiRepository
                    .CheckIfOrderExistAsync(order.Id, true);
                if (res)
                    return BadRequest(new ApiResponse(400, "Order already sent"));

                var apiId = await _unitOfWork.OtherApiRepository
                    .GetProductIdInApiAsync(order.Product.Id);

                var response = await _apiService
                    .SendOrderAsync(apiId, order.TotalQuantity, order.PlayerId);

                if (!response.Success)
                    return BadRequest(new ApiResponse(400, response.Message));

                var api = new ApiOrder
                {
                    Order = order,
                    ApiOrderId = response.OrderId
                };
                _unitOfWork.OtherApiRepository.AddOrder(api);
               
                    if (!await _unitOfWork.Complete()) return BadRequest(new ApiResponse(400, "Failed to receive order"));
                
                var not = new OrderAndPaymentNotification
                {
                    User = order.User,
                    Order = order
                };
                await _notificationService.NotifyUserByEmail(order.User.Email, _unitOfWork, not,
                    "Order status notification", "order received");

                return Ok(new ApiResponse(200,"Waiting"));

            }

            if (order.OrderType.ToLower() == "vip")
            {
                order.User.TotalPurchasing -= order.TotalPrice;
                order.User.TotalForVIPLevel -= order.TotalPrice;
                order.User.Balance += order.TotalPrice;
                order.User.VIPLevel = await _unitOfWork.VipLevelRepository
                    .GetVipLevelForPurchasingAsync(order.User.TotalForVIPLevel);

                if (!order.CanChooseQuantity)
                {
                    if (order.Product != null)
                        order.TotalPrice = await SomeUsefulFunction.CalcTotalPriceCannotChooseQuantity
                            (order.TotalQuantity, order.Product, order.User, _unitOfWork);

                    //   Console.WriteLine("price after : " + order.TotalPrice + "\n");
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
            }

            if (await _unitOfWork.Complete())
            {
                var not = new OrderAndPaymentNotification
                {
                    User = order.User,
                    Order = order
                };
                await _notificationService.NotifyUserByEmail(order.User.Email, _unitOfWork, not,
                    "Order status notification", "order done");

                return Ok(new ApiResponse(200,
                    order.Status != 3 ? "approved successfully" : "the user have no enough money"));
            }

            return BadRequest(new ApiResponse(400, "Failed to approve order"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpPost("reject-wrong/{orderId:int}")]
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

            if (order.OrderType.ToLower() != "vip" && type == "rejected")
                return BadRequest(new ApiResponse(400, "can't reject this order"));

            order.Status = type == "reject" ? 2 : 3;

            order.Notes = notes;

            if (!await _unitOfWork.Complete()) return BadRequest(new ApiResponse(400, "Failed to reject order"));

            var not = new OrderAndPaymentNotification
            {
                User = order.User,
                Order = order
            };
            await _notificationService.NotifyUserByEmail(order.User.Email, _unitOfWork, not,
                "Order status notification", "order rejected");

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

/*
 0 not canceled
 1 canceled but not confirmed by admin
 2 canceled and accepted by admin
 3 canceled but rejected by admin
 
    [HttpPost("approve-cancelation")]
    public async Task<ActionResult> ApproveCancel(int orderId)
    {
        var order = await _unitOfWork.OrdersRepository.GetOrderByIdAsync(orderId);

        if (order is null) return NotFound(new ApiResponse(404, "order not found"));

        if (order.StatusIfCanceled != 1 || order.Status != 0)
            return BadRequest(new ApiResponse(400, "can't cancel this order"));

        var price = order.TotalPrice;

        order.User.Balance += price;
        order.User.TotalPurchasing -= price;
        order.User.TotalForVIPLevel -= price;
        order.StatusIfCanceled = 2;

        order.User.VIPLevel =
            await _unitOfWork.VipLevelRepository.GetVipLevelForPurchasingAsync(order.User.TotalForVIPLevel);

        order.Notes = "accept";
        order.StatusIfCanceled = 2;

        if (await _unitOfWork.Complete()) return Ok(new ApiResponse(200, "approved"));
        return BadRequest(new ApiResponse(400, "Failed to cancel order"));
    }

    [HttpPost("reject-cancelation")]
    public async Task<ActionResult> RejectCancelation(int orderId)
    {
        var order = await _unitOfWork.OrdersRepository.GetOrderByIdAsync(orderId);
        if (order is null) return NotFound(new ApiResponse(404, "order not found"));

        if (order.StatusIfCanceled != 1 || order.Status != 0)
            return BadRequest(new ApiResponse(400, "can't cancel this order"));

        order.StatusIfCanceled = 3;
        order.Notes = "the cancelation order is rejected by admin";

        if (await _unitOfWork.Complete()) return Ok(new ApiResponse(200, "Rejected"));
        return BadRequest(new ApiResponse(400, "Failed to reject cancel order"));
    }

    [HttpGet("get-canceled-orders")]
    public async Task<ActionResult<List<OrderAdminDto>>> CanceledOrders(string? userEmail)
    {
        if (userEmail != null)
        {
            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(userEmail);

            if (user == null)
                return NotFound(new ApiResponse(404, "user not found"));
        }

        var orders = await _unitOfWork.OrdersRepository.GetCanceledOrdersRequestAsync(userEmail);

        return Ok(new ApiOkResponse(orders));
    }*/
}