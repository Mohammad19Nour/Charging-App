﻿using AutoMapper;
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
    List<string> status = new List<string> { "Pending", "Succeed", "Rejected", "Wrong", "Received", "Cancelled" };

    private static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private static SemaphoreSlim _semaphore1 = new SemaphoreSlim(1, 1);
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
            await _semaphore.WaitAsync();
            var order = await _unitOfWork.OrdersRepository.GetOrderByIdAsync(orderId);

            if (order is null)
            {
                _semaphore.Release();
                return NotFound(new ApiResponse(401, "this order doesn't exist"));
            }

            if (order.Status == 4)
            {
                _semaphore.Release();
                return BadRequest(new ApiResponse(400, "this order already received"));
            }

            if (order.Status != 0)
            {
                _semaphore.Release();
                return BadRequest(new ApiResponse(400, "this order already checked"));
            }

            var tmp = await _unitOfWork.OtherApiRepository
                .CheckIfProductExistAsync(order.Product.Id, true);


            if (tmp)
            {
                var res = await _unitOfWork.OtherApiRepository
                    .CheckIfOrderExistAsync(order.Id, true);
                if (res)
                {
                    _semaphore.Release();
                    return BadRequest(new ApiResponse(400, "Order already sent to another site"));
                }

                var apiId = await _unitOfWork.OtherApiRepository
                    .GetProductIdInApiAsync(order.Product.Id);

                var response = await _apiService
                    .SendOrderAsync(apiId, order.TotalQuantity, order.PlayerId);

                if (!response.Success)
                {
                    _semaphore.Release();
                    return BadRequest(new ApiResponse(400, response.Message));
                }

                var api = new ApiOrder
                {
                    Order = order,
                    ApiOrderId = response.OrderId
                };
                _unitOfWork.OtherApiRepository.AddOrder(api);
            }

            order.Status = 4;

            var not = new OrderAndPaymentNotification
            {
                User = order.User,
                Order = order
            };
            await _notificationService.NotifyUserByEmail(order.User.Email, _unitOfWork, not,
                "Order status notification", getDetails(order));

            _semaphore.Release();
            if (!await _unitOfWork.Complete()) return BadRequest(new ApiResponse(400, "Failed to Received order"));
            return Ok(new ApiResponse(200, "Received successfully"));
        }
        catch (Exception e)
        {
            _semaphore.Release();
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpPost("approve/{orderId:int}")]
    public async Task<ActionResult> Approve(int orderId)
    {
        try
        {
            await _semaphore1.WaitAsync();
            var order = await _unitOfWork.OrdersRepository.GetOrderByIdAsync(orderId);

            if (order is null)
            {
                _semaphore1.Release();
                return NotFound(new ApiResponse(401, "this order doesn't exist"));
            }

            if (order.Status == 0)
            {
                _semaphore1.Release();
                return BadRequest(new ApiResponse(400, "Please receive this order before approve it"));
            }

            if (order.Status != 4)
            {
                _semaphore1.Release();
                return BadRequest(new ApiResponse(400, "this order already checked"));
            }

            var tmp = await _unitOfWork.OtherApiRepository
                .CheckIfProductExistAsync(order.Product.Id, true);

            if (tmp)
            {
                _semaphore1.Release();
                return BadRequest(new ApiResponse(400, "this order from another site"));
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
                    "Order status notification", getDetails(order));

                _semaphore1.Release();
                return Ok(new ApiResponse(200,
                    order.Status != 3 ? "approved successfully" : "the user have no enough money"));
            }

            _semaphore1.Release();
            return BadRequest(new ApiResponse(400, "Failed to approve order"));
        }
        catch (Exception e)
        {
            _semaphore1.Release();
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
                "Order status notification", getDetails(order));

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
    private Dictionary<string, dynamic> getDetails(Order order)
    {
        return new Dictionary<string, dynamic>
        {
            { "orderId", order.Id },
            { "status", "order "+status[order.Status] },
        };
    }
}