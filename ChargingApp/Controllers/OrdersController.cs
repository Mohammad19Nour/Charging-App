using AutoMapper;
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

    public OrdersController(IUnitOfWork unitOfWork, IMapper mapper
        , IPhotoService photoService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _photoService = photoService;
    }

    [Authorize(Policy = "Required_Normal_Role")]
    [HttpGet("normal-my-order")]
    public async Task<ActionResult<IEnumerable<NormalOrderDto>>> GetMyOrdersNormal()
    {
        try
        {
            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(User.GetEmail());

            if (user is null) return BadRequest(new ApiResponse(401));

            return Ok(new ApiOkResponse(await _unitOfWork.OrdersRepository.GetNormalUserOrdersAsync(user.Id)));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [Authorize(Policy = "Required_VIP_Role")]
    [HttpGet("vip-my-order")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetMyOrdersVip()
    {
        try
        {
            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(User.GetEmail());

            if (user is null) return BadRequest(new ApiResponse(401));

            return Ok(new ApiOkResponse(await _unitOfWork.OrdersRepository.GetVipUserOrdersAsync(user.Id)));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [Authorize(Policy = "Required_VIP_Role")]
    [HttpPost("vip-order")]
    public async Task<ActionResult> PlaceOrderVip([FromBody] NewOrderDto dto)
    {
        try
        {
            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(User.GetEmail());
            if (user is null) return BadRequest(new ApiResponse(401));

            var product = await _unitOfWork.ProductRepository.GetProductByIdAsync(dto.ProductId);

            if (product is null)
                return BadRequest(new ApiResponse(404, "the product is not found"));

            if (!CheckIfAvailable(product))
                return BadRequest(new ApiResponse(404, "this product is not available now"));

            if (dto.PlayerName.Length == 0)
                return BadRequest(new ApiResponse(400, "player name is required"));

            if (dto.Quantity < product.MinimumQuantityAllowed)
                return BadRequest(new ApiResponse(400,
                    "the minimum quantity you can chose is " + product.MinimumQuantityAllowed));

            var order = new Order
            {
                Product = product,
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
                order.TotalPrice = await SomeUsefulFunction.CalcTotalPriceCannotChooseQuantity((int)dto.Quantity,
                    product,
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
            order.User.VIPLevel = await _unitOfWork.VipLevelRepository
                .GetVipLevelForPurchasingAsync(order.User.TotalForVIPLevel);

            _unitOfWork.OrdersRepository.AddOrder(order);

            if (!await _unitOfWork.Complete()) return BadRequest(new ApiResponse(400, "Something went wrong"));
            var res = _mapper.Map<OrderDto>(order);

            return Ok(new ApiOkResponse(res));

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    // new order 
    [Authorize(Policy = "RequiredNormalRole")]
    [HttpPost("normal-order")]
    public async Task<IActionResult> PlaceOrder([FromForm] NewNormalOrderDto dto)
    {
        try
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
                Product = product,
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
                    SomeUsefulFunction.CalcTotalPriceCannotChooseQuantity(
                        dto.Quantity, product, user, _unitOfWork);
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
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [Authorize(Policy = "RequiredVIPRole")]
    [HttpDelete]
    public async Task<ActionResult> DeleteOrder(int orderId)
    {
        try
        {
            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(User.GetEmail());
            if (user is null) return Unauthorized(new ApiResponse(401));

            var order = await _unitOfWork.OrdersRepository.GetOrderByIdAsync(orderId);

            if (order is null)
                return BadRequest(new ApiResponse(400, "this order isn't exist"));

            if (order.CreatedAt.AddSeconds(60).CompareTo(DateTime.UtcNow) > 0)
                return BadRequest(new ApiResponse(400, "you can delete this order after " +
                                                       CalcSeconds(order.CreatedAt.AddSeconds(60).Second,
                                                           DateTime.UtcNow.Second) + " seconds"));

            if (order.Status != 0 && order.Status != 5)
                return BadRequest(new ApiResponse(400,
                    "you can't delete this order because it has been checked by admin"));


            if (order.Status == 5)
                return BadRequest(new ApiResponse(400, "you can't delete this order because it has been cancelled"));

            order.User.Balance += order.TotalPrice;
            order.User.TotalPurchasing -= order.TotalPrice;
            order.User.TotalForVIPLevel -= order.TotalPrice;
            order.User.VIPLevel = await _unitOfWork.VipLevelRepository
                .GetVipLevelForPurchasingAsync(order.User.TotalForVIPLevel);

            order.Status = 5;
            order.StatusIfCanceled = 2;

            if (await _unitOfWork.Complete())
                return Ok(new ApiResponse(201, "Cancelled successfully "));

            return BadRequest(new ApiResponse(400, "Something went wrong during deleting the order"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
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