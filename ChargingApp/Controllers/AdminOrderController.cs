using AutoMapper;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Helpers;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChargingApp.Controllers;

[Authorize(Policy = "Required_AnyAdmin_Role")]
public class AdminOrderController : AdminController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AdminOrderController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
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
                    continue;
                t.User.TotalPurchasing -= t.TotalPrice;
                t.User.TotalForVIPLevel -= t.TotalPrice;
                t.User.Balance += t.TotalPrice;
                t.User.VIPLevel = await _unitOfWork.VipLevelRepository
                    .GetVipLevelForPurchasingAsync(t.User.TotalForVIPLevel);
            }

            foreach (var t in res)
            {
                if (t.OrderType.ToLower() == "normal")
                {
                    tmp.Add(_mapper.Map<PendingOrderDto>(t));
                }
                else
                {
                    if (t.Product is null) continue;
                    if (!t.CanChooseQuantity)
                    {
                        var pr = await PriceForVIP.CannotChooseQuantity
                            (t.TotalQuantity, t.Product, t.User, _unitOfWork);
                        t.TotalPrice = pr;
                    }
                    else
                    {
                        t.TotalQuantity = await
                            PriceForVIP.CalcTotalQuantity(t.Product.Quantity, t.Product
                                , t.User, _unitOfWork);

                        var specificPrice = await _unitOfWork.SpecificPriceForUserRepository
                            .GetProductPriceForUserAsync(t.Product.Id, t.User);

                        if (specificPrice != null)
                        {
                            t.TotalPrice = (decimal)specificPrice;
                            t.TotalQuantity = t.Quantity;
                        }
                        else
                            t.TotalPrice = t.Product.Price;

                        t.User.TotalPurchasing += t.TotalPrice;
                        t.User.TotalForVIPLevel += t.TotalPrice;
                    }

                    if (t.User.Balance >= t.TotalPrice)
                    {
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
            return Ok(new ApiOkResponse<List<PendingOrderDto>>(tmp));
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
            res = res.Where(x => x.User.Email.ToLower() == email.ToLower()).ToList();
            foreach (var t in res)
            {
                if (t.OrderType.ToLower() == "normal")
                    continue;
                t.User.TotalPurchasing -= t.TotalPrice;
                t.User.TotalForVIPLevel -= t.TotalPrice;
                t.User.Balance += t.TotalPrice;
                t.User.VIPLevel = await _unitOfWork.VipLevelRepository
                    .GetVipLevelForPurchasingAsync(t.User.TotalForVIPLevel);
            }

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
                        var pr = await PriceForVIP.CannotChooseQuantity
                            (t.TotalQuantity, t.Product, t.User, _unitOfWork);
                        t.TotalPrice = pr;
                    }
                    else
                    {
                        t.TotalQuantity = await
                            PriceForVIP.CalcTotalQuantity(t.Product.Quantity, t.Product
                                , t.User, _unitOfWork);

                        var specificPrice = await _unitOfWork.SpecificPriceForUserRepository
                            .GetProductPriceForUserAsync(t.Product.Id, t.User);

                        if (specificPrice != null)
                        {
                            t.TotalPrice = (decimal)specificPrice;
                            t.TotalQuantity = t.Quantity;
                        }
                        else
                            t.TotalPrice = t.Product.Price;

                        t.User.TotalPurchasing += t.TotalPrice;
                        t.User.TotalForVIPLevel += t.TotalPrice;
                    }

                    if (t.User.Balance >= t.TotalPrice)
                    {
                        t.User.Balance -= t.TotalPrice;
                        t.User.VIPLevel = await _unitOfWork.VipLevelRepository
                            .GetVipLevelForPurchasingAsync(t.User.TotalForVIPLevel);
                    }
                    else
                    {
                        t.User.TotalPurchasing -= t.TotalPrice;
                        t.User.TotalForVIPLevel -= t.TotalPrice;
                    }

                    var x = new PendingOrderDto();
                    _mapper.Map(t, x);
                    tmp.Add(x);
                }
            }

            tmp = tmp.OrderByDescending(x => x.CreatedAt).ToList();
            return Ok(new ApiOkResponse<List<PendingOrderDto>>(tmp));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpGet("done-orders")]
    public async Task<ActionResult<List<DoneOrderDto>>> DoneOrders([FromQuery] DateQueryDto dto)
    {
        var (ans, msg) = SomeUsefulFunction.CheckDate(dto);

        if (!ans) return BadRequest(new ApiResponse(400, msg));

        var result = await _unitOfWork.OrdersRepository.GetDoneOrders(dto, null);
        var orders = _mapper.Map<List<DoneOrderDto>>(result);
        return Ok(new ApiOkResponse<object>(new { Doneorders = orders}));
    }

    // delete done orders in last 6 months
    [HttpDelete("delete-orders")]
    public async Task<ActionResult<bool>> DeleteOrders()
    {
        try
        {
            var dateTime = DateTime.UtcNow.AddDays(-183);

            var orderQuery = _unitOfWork.OrdersRepository.GetQueryable();

            orderQuery = orderQuery.Where(x => x.Status == 1)
                .Where(x => x.CreatedAt < dateTime);

            var orders = await orderQuery.ToArrayAsync();

            foreach (var order in orders)
            {
                var notList = await _unitOfWork.NotificationRepository
                    .GetOrdersNotifications(order.Id);
                foreach (var not in notList)
                    _unitOfWork.NotificationRepository.DeleteNotification(not);
            }

            _unitOfWork.OrdersRepository.DeleteOrders(orders);

            if (await _unitOfWork.Complete())
                return Ok(new ApiResponse(200, "All orders Before six months has been deleted successfully"));

            if (!_unitOfWork.HasChanges())
                return Ok(new ApiResponse(200, "There are no orders before six months to be deleted"));

            return BadRequest(new ApiResponse(400, "Failed to delete orders"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}