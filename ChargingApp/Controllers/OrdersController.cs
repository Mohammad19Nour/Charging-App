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
    private readonly IPhotoService _photoService;

    public OrdersController(IUnitOfWork unitOfWork, DataContext context, IMapper mapper
        , IPhotoService photoService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _photoService = photoService;
    }

    [Authorize(Policy = "RequiredNormalRole")]
    [HttpGet("normal-my-order")]
    public async Task<ActionResult<IEnumerable<NormalOrderDto>>> GetMyOrdersNormal()
    {
        var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(User.GetEmail());
       
        if (user is null) return BadRequest(new ApiResponse(401));

        return Ok(new ApiOkResponse(await _unitOfWork.OrdersRepository.GetNormalUserOrdersAsync(user.Id)));
    }

    [Authorize(Policy = "RequiredVIPRole")]
    [HttpGet("vip-my-order")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetMyOrdersVip()
    {
        var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(User.GetEmail());

        if (user is null) return BadRequest(new ApiResponse(401));

        return Ok(new ApiOkResponse(await _unitOfWork.OrdersRepository.GetVipUserOrdersAsync(user.Id)));
    }

    [Authorize(Policy = "RequiredVIPRole")]
    [HttpPost("vip-order")]
    public async Task<ActionResult> PlaceOrderVip([FromBody] NewOrderDto dto)
    {
        var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(User.GetEmail());
        if (user is null) return BadRequest(new ApiResponse(401));
        
        var product = await _unitOfWork.ProductRepository.GetProductByIdAsync(dto.ProductId);

        if (product is null)
            return BadRequest(new ApiResponse(404, "the product is not found"));

        if (!CheckIfAvailable(product))
            return BadRequest(new ApiResponse(404, "this product is not available now"));

        var lastOrder = await _unitOfWork.OrdersRepository.GetLastOrderForUserByIdAsync(user.Id);

        if (lastOrder != null)
        {
            if (lastOrder.CreatedAt.AddSeconds(15).CompareTo(DateTime.UtcNow) > 0)
            {
                return BadRequest(new ApiResponse(400,
                    "you can make a new order after " +
                    CalcSeconds(lastOrder.CreatedAt.AddSeconds(15).Second, DateTime.UtcNow.Second) + " seconds"));
            }
        }

        var resu = await _unitOfWork.OrdersRepository.CheckPendingOrdersForUserByEmailAsync(user.Email);

        if (resu)
        {
            return BadRequest(new ApiResponse(400, "you have pending order"));
        }
        /*  if (product.CanChooseQuantity)
          {
              if (Math.Abs(product.Quantity - dto.Quantity) > 0.001)
                  return BadRequest(new ApiResponse(400, "you can't choose this quantity"));
          }*/

        if (dto.PlayerName.Length == 0)
            return BadRequest(new ApiResponse(400, "player name is required"));

        if (dto.Quantity < product.MinimumQuantityAllowed)
            return BadRequest(new ApiResponse(400,
                "the minimum quantity you can chose is " + product.MinimumQuantityAllowed));

        var order = new Order
        {
            ProductArabicName = product.ArabicName,
            ProductEnglishName = product.EnglishName,
            CanChooseQuantity = product.CanChooseQuantity,
            Quantity = product.Quantity,
            Price = product.Price,
            User = user,
            PlayerId = dto.PlayerId,
            OrderType = "VIP",
            PlayerName = dto.PlayerName
        };

        if (!product.CanChooseQuantity)
        {
            order.TotalPrice = await SomeUsefulFunction.CalcTotalPriceCannotChooseQuantity((int)dto.Quantity, product,
                user,
                _unitOfWork);
            order.TotalQuantity = dto.Quantity;
        }
        else
        {
            order.TotalQuantity = await
                SomeUsefulFunction.CalcTotalQuantity(product.Quantity, product, user, _unitOfWork);

            var specificPrice = await _unitOfWork.SpecificPriceForUserRepository
                .GetProductPriceForUserAsync(product.Id, user);

            order.TotalPrice = specificPrice ?? product.Price;
            order.Quantity = product.Quantity;
        }

        if (order.TotalPrice > user.Balance)
            return BadRequest(new ApiResponse(400, "you have no enough money to do this"));

        user.Balance -= order.TotalPrice;
        _unitOfWork.OrdersRepository.AddOrder(order);

        if (await _unitOfWork.Complete())
        {
            var res = _mapper.Map<OrderDto>(order);

            return Ok(new ApiOkResponse(res));
        }

        return BadRequest(new ApiResponse(400, "Something went wrong"));
    }

    // new order 
    [Authorize(Policy = "RequiredNormalRole")]
    [HttpPost("normal-order")]
    public async Task<IActionResult> PlaceOrder([FromForm] NewNormalOrderDto dto)
    {
        var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(User.GetEmail());

        if (user is null) return BadRequest(new ApiResponse(401));

        var product = await _unitOfWork.ProductRepository.GetProductByIdAsync(dto.ProductId);

        if (product is null)
            return BadRequest(new ApiResponse(404, "the product is not found"));

        if (!CheckIfAvailable(product))
            return BadRequest(new ApiResponse(404, "this product is not available now"));

        var paymentGateway =
            await _unitOfWork.PaymentGatewayRepository.GetPaymentGatewayByNameAsync(dto.PaymentGateway);

        if (paymentGateway is null)
            return BadRequest(new ApiResponse(404, "payment gateway isn't exist"));

        if (dto.PlayerName.Length == 0)
            return BadRequest(new ApiResponse(400, "player name is required"));

        /*      if (product.CanChooseQuantity)
              {
                  if (Math.Abs(product.Quantity - dto.Quantity) > 0.001)
                      return BadRequest(new ApiResponse(400, "you can't choose this quantity"));
              }
      */
        if (dto.Quantity < product.MinimumQuantityAllowed)
            return BadRequest(new ApiResponse(400,
                "the minimum quantity you can chose is " + product.MinimumQuantityAllowed));

        var result = await _photoService.AddPhotoAsync(dto.ReceiptPhoto);

        if (!result.Success)
            return BadRequest(new ApiResponse(400, result.Message));

        var photo = new Photo
        {
            Url = result.Url
        };
        var order = new Order
        {
            ProductArabicName = product.ArabicName,
            ProductEnglishName = product.EnglishName,
            CanChooseQuantity = product.CanChooseQuantity,
            Quantity = product.Quantity,
            Price = product.Price,
            User = user,
            PlayerId = dto.PlayerId,
            PaymentGateway = paymentGateway,
            OrderType = "Normal",
            Photo = photo,
            PlayerName = dto.PlayerName.ToLower(),
        };

        if (!product.CanChooseQuantity)
        {
            order.TotalPrice = await
                SomeUsefulFunction.CalcTotalPriceCannotChooseQuantity((int)dto.Quantity, product, user, _unitOfWork);
            order.TotalQuantity = dto.Quantity;
        }
        else
        {
            order.TotalQuantity = await
                SomeUsefulFunction.CalcTotalQuantity((int)dto.Quantity, product, user, _unitOfWork);

            var specificPrice = await _unitOfWork.SpecificPriceForUserRepository
                .GetProductPriceForUserAsync(product.Id, user);

            order.TotalPrice = specificPrice ?? product.Price;
            order.Quantity = product.Quantity;
        }

        _unitOfWork.OrdersRepository.AddOrder(order);

        if (!await _unitOfWork.Complete())
            return BadRequest(new ApiResponse(400, "Something went wrong"));

        var res = _mapper.Map<NormalOrderDto>(order);
        return Ok(new ApiOkResponse(res));
    }

    [Authorize(Policy = "RequiredVIPRole")]
    [HttpDelete]
    public async Task<ActionResult> DeleteOrder(int orderId)
    {
        var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(User.GetEmail());
        if (user is null) return Unauthorized(new ApiResponse(401));

        var order = await _unitOfWork.OrdersRepository.GetOrderByIdAsync(orderId);

        if (order is null)
            return BadRequest(new ApiResponse(400, "this order isn't exist"));

        if (order.CreatedAt.AddSeconds(15).CompareTo(DateTime.UtcNow) < 0)
            return BadRequest(new ApiResponse(400, "you can't delete this order because it's been 15 seconds"));

        if (order.Status != 0)
            return BadRequest(new ApiResponse(400, "you can't delete this order because it has been checked by admin"));

        if (order.StatusIfCanceled != 0)
            return BadRequest(new ApiResponse(400, "you can't delete this order because it has been canceled"));

        order.StatusIfCanceled = 1;
        // _unitOfWork.OrdersRepository.DeleteOrder(order);

        if (await _unitOfWork.Complete())
            return Ok(new ApiResponse(201, "wait the admin to confirm the cancelation"));

        return BadRequest(new ApiResponse(400, "Something went wrong during deleting the order"));
    }

    private bool CheckIfAvailable(Product product)
    {
        return !product.CanChooseQuantity ? product.Available : product.Category.Available;
    }

    private static int CalcSeconds(int second, int nowSecond)
    {
        return nowSecond <= second ? Math.Max(1, second - nowSecond) : Math.Max(second + 60 - nowSecond, 1);
    }
}