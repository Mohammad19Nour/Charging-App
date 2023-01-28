using AutoMapper;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Helpers;
using ChargingApp.Interfaces;
using ChargingApp.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ChargingApp.Controllers;

public class AdminOrderController : AdminController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubContext _presenceHub;
    private readonly PresenceTracker _tracker;
    private readonly IMapper _mapper;

    public AdminOrderController(IUnitOfWork unitOfWork
        , IMapper mapper)
    {
        //   , IHubContext presenceHub, PresenceTracker tracker
        _unitOfWork = unitOfWork;
        //    _presenceHub = presenceHub;
        //  _tracker = tracker;
        _mapper = mapper;
    }

    [HttpPost("receive-order")]
    public async Task<ActionResult> Receive(int orderId)
    {
        var order = await _unitOfWork.OrdersRepository.GetOrderByIdAsync(orderId);

        if (order is null) return NotFound(new ApiResponse(401, "this order doesn't exist"));

        if (order.Status == 4)
            return BadRequest(new ApiResponse(400, "this order already received"));

        if (order.Status != 0)
            return BadRequest(new ApiResponse(400, "this order already checked"));

        order.Status = 4;
        /*
        var connections = await _tracker.GetConnectionsForUser(order.User.Email);

        if (connections != null)
        {
            await _presenceHub.Clients.Clients(connections)
                .SendAsync("NewOrderNotification","new order");
        }
        else
        {
            _unitOfWork.NotificationRepository.AddNotification(new OrderAndPaymentNotification()
            {
                User = order.User,
                Order = order
            });
        }*/

        if (await _unitOfWork.Complete())
        {
            return Ok(new ApiResponse(200, "Received successfully"));
        }

        return BadRequest(new ApiResponse(400, "Failed to Received order"));
    }

    [HttpPost("approve/{orderId:int}")]
    public async Task<ActionResult> Approve(int orderId)
    {
        var order = await _unitOfWork.OrdersRepository.GetOrderByIdAsync(orderId);

        if (order is null) return NotFound(new ApiResponse(401, "this order doesn't exist"));

        if (order.Status == 0)
            return BadRequest(new ApiResponse(400, "Please receive this order before approve it"));
        
        if (order.Status != 4)
            return BadRequest(new ApiResponse(400, "this order already checked"));

        // var roles = order.User.
        if (order.OrderType.ToLower() == "vip")
        {
            order.User.TotalPurchasing -= order.TotalPrice;
            order.User.TotalForVIPLevel -= order.TotalPrice;
            order.User.Balance += order.TotalPrice;
            order.User.VIPLevel = await _unitOfWork.VipLevelRepository
                .GetVipLevelForPurchasingAsync(order.User.TotalForVIPLevel);
             
             Console.WriteLine("balance : " + order.User.Balance+"\n");
             Console.WriteLine("TotalPurchasing : " + order.User.TotalPurchasing+"\n");
             Console.WriteLine("TotalForVIPLevel : " + order.User.TotalForVIPLevel+"\n");
             
            if (!order.CanChooseQuantity)
            {
                //  Console.WriteLine("price before : " + order.TotalPrice+"\n");
                order.TotalPrice = await SomeUsefulFunction.CalcTotalPriceCannotChooseQuantity
                    (order.TotalQuantity, order.Product, order.User, _unitOfWork);

                  Console.WriteLine("price after : " + order.TotalPrice+"\n");
            }
            else
            {
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
/*
        var connections = await _tracker.GetConnectionsForUser(order.User.Email);

        if (connections != null)
        {
            await _presenceHub.Clients.Clients(connections)
                .SendAsync("NewOrderNotification", "wrong");
        }
        else
        {
            _unitOfWork.NotificationRepository.AddNotification(new OrderAndPaymentNotification()
            {
                User = order.User,
                Order = order
            });
        }*/

        if (await _unitOfWork.Complete())
        {
            return Ok(new ApiResponse(200,order.Status != 3 ? "approved successfully":"the user have no enough money"));
        }

        return BadRequest(new ApiResponse(400, "Failed to approve order"));
    }

    [HttpPost("reject-wrong/{orderId:int}")]
    public async Task<ActionResult> RejectOrWrong(int orderId, string type, string notes = "")
    {
        //type either wrong or reject

        type = type.ToLower();

        if (type != "wrong" && type != "reject")
            return BadRequest(new ApiResponse(400, "type should be wrong or reject"));

        var order = await _unitOfWork.OrdersRepository.GetOrderByIdAsync(orderId);

        if (order is null) return NotFound(new ApiResponse(401, "this order doesn't exist"));

        if (order.Status != 4) 
            return BadRequest(new ApiResponse(400, "this order already checked"));

        if (order.OrderType.ToLower() != "vip" && type == "rejected")
            return BadRequest(new ApiResponse(400, "can't reject this order"));


        order.Status = type == "reject" ? 2 : 3;

        order.Notes = notes;

        if (order.User != null)
        {
            if (order.OrderType.ToLower() == "vip")
            {
                var user = order.User;

                user.Balance += order.TotalPrice;
                user.TotalPurchasing -= order.TotalPrice;
                user.TotalForVIPLevel -= order.TotalPrice;
                user.VIPLevel =
                    await _unitOfWork.VipLevelRepository.GetVipLevelForPurchasingAsync(user.TotalForVIPLevel);
            }
        }

       /* var connections = await _tracker.GetConnectionsForUser(order.User.Email);

        if (connections != null)
        {
            await _presenceHub.Clients.Clients(connections)
                .SendAsync("NewOrderNotification", "new order");
        }
        else
        {
            _unitOfWork.NotificationRepository.AddNotification(new OrderAndPaymentNotification()
            {
                User = order.User,
                Order = order
            });
        }*/

        if (await _unitOfWork.Complete())
        {
            return Ok(new ApiResponse(200, "Done successfully"));
        }

        return BadRequest(new ApiResponse(400, "Failed"));
    }

    [HttpGet("pending-orders")]
    public async Task<ActionResult<List<PendingOrderDto>>> GetPendingOrders()
    {
        var res = await _unitOfWork.OrdersRepository.GetPendingOrdersAsync();
        var tmp = new List<PendingOrderDto>();
        // Console.WriteLine(res.Count+"\n*\n");

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

        return Ok(new ApiOkResponse(tmp));
    }

    [HttpGet("pending-orders/{email}")]
    public async Task<ActionResult<List<PendingOrderDto>>> GetPendingOrders(string email)
    {
        var res = await _unitOfWork.OrdersRepository.GetPendingOrdersAsync(email);

        var tmp = new List<PendingOrderDto>();
        // Console.WriteLine(res.Count+"\n*\n");

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

        return Ok(new ApiOkResponse(tmp));
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