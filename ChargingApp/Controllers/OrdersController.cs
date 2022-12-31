using AutoMapper;
using ChargingApp.Data;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Extentions;
using ChargingApp.Helpers;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

[Authorize]
public class OrdersController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public OrdersController(IUnitOfWork unitOfWork, DataContext context, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [Authorize(Policy = "RequiredNormalRole")]
    [HttpGet("normal-my-order")]
    public async Task<ActionResult<IEnumerable<NormalOrderDto>>> GetMyOrdersNormal()
    {
        var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(User.GetEmail());
        if (user is null) return BadRequest(new ApiResponse(401));
        if (user.VIPLevel != 0)
            return BadRequest(new ApiResponse(400, "you're not allowed to make this request"));

        return Ok(new ApiOkResponse(await _unitOfWork.OrdersRepository.GetNormalUserOrdersAsync(user.Id)));
    }

    [HttpGet("vip-my-order")]
    [Authorize(Policy = "RequiredVIPRole")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetMyOrdersVip()
    {
        var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(User.GetEmail());
        if (user is null) return BadRequest(new ApiResponse(401));
        if (user.VIPLevel == 0)
            return BadRequest(new ApiResponse(400, "you're not allowed to make this request"));

        return Ok(new ApiOkResponse(await _unitOfWork.OrdersRepository.GetVipUserOrdersAsync(user.Id)));
    }

    [HttpPost("vip-order")]
    [Authorize(Policy = "RequiredVIPRole")]
    public async Task<ActionResult> PlaceOrderVip([FromBody] NewOrderDto dto)
    {
        var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(User.GetEmail());
        if (user is null) return BadRequest(new ApiResponse(401));

        if (user.VIPLevel == 0)
            return BadRequest(new ApiResponse(400, "you're not allowed to make this request"));

        var product = await _unitOfWork.ProductRepository.GetProductByIdAsync(dto.ProductId);

        if (product is null)
            return BadRequest(new ApiResponse(404, "the product is not found"));

        if (!CheckIfAvailable(product))
            return BadRequest(new ApiResponse(404, "this product is not available now"));

        var lastOrder = await _unitOfWork.OrdersRepository.GetLastOrderForUserByIdAsync(user.Id);

        if (lastOrder != null)
        {
            if (lastOrder.CreatedAt.AddMinutes(1).CompareTo(DateTime.UtcNow) > 0)
            {
                return BadRequest(new ApiResponse(400,
                    "you can make a new order after " +
                    CalcSeconds(lastOrder.CreatedAt.AddMinutes(1).Second, DateTime.UtcNow.Second) + " seconds"));
            }
        }

        if (product.CanChooseQuantity)
        {
            if (product.AvailableQuantities.FirstOrDefault(x => x.Value == dto.Quantity) == null)
                return BadRequest(new ApiResponse(400, "you can't choose this quantity"));
        }

        if (dto.Quantity < product.MinimumQuantityAllowed)
            return BadRequest(new ApiResponse(400,
                "the minimum quantity you can chose is " + product.MinimumQuantityAllowed));

        var order = new Order
        {
            Product = product,
            User = user,
            PlayerId = dto.PlayerId,
            TotalPrice = SomeUsefulFunction.CalcTotalPrice(dto.Quantity, product.Price, user,
                await _unitOfWork.VipLevelRepository.GetAllVipLevelsAsync()),
            Quantity = dto.Quantity,
            OrderType = "VIP",
            Succeed = true,
            Checked = true
        };
        if (order.TotalPrice > user.Balance)
            return BadRequest(new ApiResponse(400, "you have no enough money to do this"));

        user.Balance -= order.TotalPrice;
        _unitOfWork.OrdersRepository.AddOrder(order);

        if (await _unitOfWork.Complete())
            return Ok(new ApiOkResponse(_mapper.Map<OrderDto>(order)));

        return BadRequest(new ApiResponse(400, "Something went wrong"));
    }

    // new order 
    [HttpPost("normal-order")]
    [Authorize(Policy = "RequiredNormalRole")]
    public async Task<IActionResult> PlaceOrder([FromBody] NewNormalOrderDto dto)
    {
        var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(User.GetEmail());
      
        if (user is null) return BadRequest(new ApiResponse(401));
        
        if (user.VIPLevel != 0)
            return BadRequest(new ApiResponse(400, "you're not allowed to make this request"));

        var product = await _unitOfWork.ProductRepository.GetProductByIdAsync(dto.ProductId);
        
        if (product is null)
            return BadRequest(new ApiResponse(404, "the product is not found"));

        if (!CheckIfAvailable(product))
            return BadRequest(new ApiResponse(404, "this product is not available now"));

        var paymentGateway =
            await _unitOfWork.PaymentGatewayRepository.GetPaymentGatewayByNameAsync(dto.PaymentGateway);

        if (paymentGateway is null)
            return BadRequest(new ApiResponse(404, "payment gateway isn't exist"));

        if (product.CanChooseQuantity)
        {
            if (product.AvailableQuantities.FirstOrDefault(x => x.Value == dto.Quantity) == null)
                return BadRequest(new ApiResponse(400, "you can't choose this quantity"));
        }

        if (dto.Quantity < product.MinimumQuantityAllowed)
            return BadRequest(new ApiResponse(400,
                "the minimum quantity you can chose is " + product.MinimumQuantityAllowed));

        var order = new Order
        {
            Product = product,
            User = user,
            PlayerId = dto.PlayerId,
            TransferNumber = dto.TransferNumber,
            TotalPrice = dto.Quantity * product.Price,
            Quantity = dto.Quantity,
            PaymentGateway = paymentGateway,
            OrderType = "Normal",
            Succeed = true,
            Checked = true,
            // CreatedAt = DateTime.Now.tos
        };
        _unitOfWork.OrdersRepository.AddOrder(order);
        if (await _unitOfWork.Complete())
        {
            //change to return dto
            return Ok(new ApiOkResponse(_mapper.Map<NormalOrderDto>(order)));
        }

        return BadRequest(new ApiResponse(400, "Something went wrong"));
    }

    [HttpDelete]
    [Authorize(Policy = "RequiredVIPRole")]
    public async Task<ActionResult> DeleteOrder(int orderId)
    {
        var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(User.GetEmail());
        if (user is null) return Unauthorized(new ApiResponse(401));

        if (user.VIPLevel == 0)
            return BadRequest(new ApiResponse(400, "you're not allowed to do this action"));

        var order = await _unitOfWork.OrdersRepository.GetOrderByIdAsync(orderId);

        if (order is null)
            return BadRequest(new ApiResponse(400, "this order isn't exist"));

        if (order.CreatedAt.AddMinutes(1).CompareTo(DateTime.UtcNow) < 0)
            return BadRequest(new ApiResponse(400, "you can't delete this order because it's been 1 minute"));

        var price = order.TotalPrice;
        user.Balance += price;
        user.TotalPurchasing -= price;

        user.VIPLevel =
            await _unitOfWork.VipLevelRepository.GetVipLevelForPurchasingAsync(user.TotalPurchasing);

        _unitOfWork.OrdersRepository.DeleteOrder(order);

        if (await _unitOfWork.Complete())
            return Ok(new ApiResponse(201, "order deleted successfully"));
        return BadRequest(new ApiResponse(400, "Something went wrong during deleting the order"));
    }
    
    private bool CheckIfAvailable(Product product)
    {
        if (!product.CanChooseQuantity)
        {
            return product.Available;
        }

        return product.Category.Available;
    }

    private int CalcSeconds(int second, int nowSecond)
    {
        if (nowSecond <= second) return Math.Max(1, second - nowSecond);
        return Math.Max(second + 60 - nowSecond, 1);
    }
}