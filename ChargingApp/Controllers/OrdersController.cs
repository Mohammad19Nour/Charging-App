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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ChargingApp.Controllers;

[Authorize]
public class OrdersController : BaseApiController
{
    private readonly IUserRepository _userRepository;
    private readonly IOrdersRepository _ordersRepository;
    private readonly IProductRepository _productRepo;
    private readonly IPaymentGatewayRepository _paymentGatewayRepo;
    private readonly IVipLevelRepository _vipLevelRepo;
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public OrdersController(IUserRepository userRepository, IOrdersRepository ordersRepository,
        IProductRepository productRepo, IPaymentGatewayRepository paymentGatewayRepo,
        IVipLevelRepository vipLevelRepo, DataContext context,IMapper mapper
    )
    {
        _userRepository = userRepository;
        _ordersRepository = ordersRepository;
        _productRepo = productRepo;
        _paymentGatewayRepo = paymentGatewayRepo;
        _vipLevelRepo = vipLevelRepo;
        _context = context;
        _mapper = mapper;
    }

    [HttpGet("normal-my-order")]
    public async Task<ActionResult<IEnumerable<NormalOrderDto>>> GetMyOrdersNormal()
    {
        var user = await _userRepository.GetUserByEmailAsync(User.GetEmail());
        if (user is null) return BadRequest(new ApiResponse(401));
        if (user.VIPLevel != 0)
            return BadRequest(new ApiResponse(400, "you're not allowed to make this request"));

        return Ok(new ApiOkResponse( await _ordersRepository.GetNormalUserOrdersAsync(user.Id)));
    }

    [HttpGet("vip-my-order")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetMyOrdersVip()
    {
        var user = await _userRepository.GetUserByEmailAsync(User.GetEmail());
        if (user is null) return BadRequest(new ApiResponse(401));
        if (user.VIPLevel == 0)
            return BadRequest(new ApiResponse(400, "you're not allowed to make this request"));

        return Ok(new ApiOkResponse(await _ordersRepository.GetVipUserOrdersAsync(user.Id)));
    }

    [HttpPost("vip-order")]
    public async Task<ActionResult> PlaceOrderVip([FromBody] NewOrderDto dto)
    {
        var user = await _userRepository.GetUserByEmailAsync(User.GetEmail());
        if (user is null) return BadRequest(new ApiResponse(401));

        if (user.VIPLevel == 0)
            return BadRequest(new ApiResponse(400, "you're not allowed to make this request"));

        var product = await _productRepo.GetProductByIdAsync(dto.ProductId);

        if (product is null)
            return BadRequest(new ApiResponse(404, "the product is not found"));

        if (!CheckIfAvailable(product))
            return BadRequest(new ApiResponse(404, "this product is not available now"));

        var lastOrder = await _ordersRepository.GetLastOrderForUserByIdAsync(user.Id);

        if (lastOrder != null)
        {
            if ((lastOrder.CreatedAt.AddMinutes(1).CompareTo(DateTime.Now) > 0) && !lastOrder.Checked)
                return BadRequest(new ApiResponse(400, "your previous order was less than a minuet old "));
        }

        if (product.CanChooseQuantity)
        {
            if (product.AvailableQuantities.FirstOrDefault(x => x.Value == dto.Quantity) == null)
                return BadRequest(new ApiResponse(400, "you can't choose this quantity"));
        }

        if (dto.Quantity < product.MinimumQuantityAllowed)
            return BadRequest(new ApiResponse(400, "the minimum quantity you can chose is " + product.MinimumQuantityAllowed));

        await using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                var order = new Order
                {
                    Product = product,
                    User = user,
                    PlayerId = dto.PlayerId,
                    TotalPrice = SomeUsefulFunction.CalcTotalePrice(dto.Quantity, product.Price, user,
                        await _vipLevelRepo.GetAllVipLevelsAsync()),
                    Quantity = dto.Quantity,
                    OrderType = "VIP",
                };
                if (order.TotalPrice > user.Balance)
                    return BadRequest(new ApiResponse(400, "you have no enough money to do this"));

                user.Balance -= order.TotalPrice;
                _ordersRepository.AddOrder(order);

              await  _context.SaveChangesAsync();
              await  transaction.CommitAsync();
                return Ok(new ApiOkResponse(_mapper.Map<OrderDto>(order)));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new ApiResponse(400, "Something went wrong"));
            }
        }
    }

// new order 
    [HttpPost("normal-order")]
    public async Task<IActionResult> PlaceOrder([FromBody] NewNormalOrderDto dto)
    {
        var user = await _userRepository.GetUserByEmailAsync(User.GetEmail());
        if (user is null) return BadRequest(new ApiResponse(401));
        if (user.VIPLevel != 0)
            return BadRequest(new ApiResponse(400, "you're not allowed to make this request"));

        var product = await _productRepo.GetProductByIdAsync(dto.ProductId);


        if (product is null)
            return BadRequest(new ApiResponse(404, "the product is not found"));

        if (!CheckIfAvailable(product))
            return BadRequest(new ApiResponse(404, "this product is not available now"));

        var lastOrder = await _ordersRepository.GetLastOrderForUserByIdAsync(user.Id);

        if (lastOrder != null)
        {
            if ((lastOrder.CreatedAt.AddMinutes(1).CompareTo(DateTime.Now) > 0) && !lastOrder.Checked)
                return BadRequest(new ApiResponse(400, "your previous order was less than a minuet old "));
        }

        var paymentGateway = await _paymentGatewayRepo.GetPaymentGatewayByNameAsync(dto.PaymentGateway);

        if (paymentGateway is null) 
            return BadRequest(new ApiResponse(404, "payment gateway isn't exist"));

        if (product.CanChooseQuantity)
        {
            if (product.AvailableQuantities.FirstOrDefault(x => x.Value == dto.Quantity) == null)
                return BadRequest(new ApiResponse(400, "you can't choose this quantity"));
        }

        if (dto.Quantity < product.MinimumQuantityAllowed)
            return BadRequest(new ApiResponse(400, "the minimum quantity you can chose is " + product.MinimumQuantityAllowed));

        var order = new Order
        {
            Product = product,
            User = user,
            PlayerId = dto.PlayerId,
            TransferNumber = dto.TransferNumber,
            TotalPrice = dto.Quantity * product.Price,
            Quantity = dto.Quantity,
            PaymentGateway = paymentGateway,
            OrderType = "Normal"
            //  CreatedAt = DateTime.Now
        };
        _ordersRepository.AddOrder(order);
        if (await _ordersRepository.SaveAllChangesAsync())
        {
            //change to return dto
            return Ok(new ApiOkResponse(_mapper.Map<NormalOrderDto>(order)));
        }

        return BadRequest(new ApiResponse(400, "Something went wrong"));
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteOrder(int orderId)
    {
        var user = await _userRepository.GetUserByEmailAsync(User.GetEmail());
        if (user is null) return Unauthorized(new ApiResponse(401));

        if (user.VIPLevel == 0)
            return BadRequest(new ApiResponse(400, "you're not allowed to do this action"));

        var order = await _ordersRepository.GetOrderByIdAsync(orderId);

        if (order is null)
            return BadRequest(new ApiResponse(400, "this order isn't exist"));

        if (order.CreatedAt.AddMinutes(1).CompareTo(DateTime.Now) < 0)
            return BadRequest(new ApiResponse(400, "you can't delete this order because it's been 1 minute"));
        using (var transaction = _context.Database.BeginTransaction())
        {
            try
            {
                var price = order.TotalPrice;
                user.Balance += price;
                user.TotalPurchasing -= price;

                user.VIPLevel = await _vipLevelRepo.GetVipLevelForPurchasingAsync(user.TotalPurchasing);

                _ordersRepository.DeleteOrderById(orderId);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new ApiResponse(201, "order deleted successfully"));
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                return BadRequest(new ApiResponse(400, "Something went wrong during deleting the order"));
            }
        }
    }


    private bool CheckIfAvailable(Product product)
    {
        if (!product.CanChooseQuantity)
        {
            return product.Available;
        }

        return product.Category.Available;
    }
}