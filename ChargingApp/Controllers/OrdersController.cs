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
    private readonly IApiService _apiService;
    private readonly IPhotoService _photoService;
    private readonly INotificationService _notificationService;

    public OrdersController(IUnitOfWork unitOfWork, IMapper mapper, IApiService apiService
        , IPhotoService photoService, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _apiService = apiService;
        _photoService = photoService;
        _notificationService = notificationService;
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

            var fromApi = await _unitOfWork.OtherApiRepository
                .CheckIfProductExistAsync(product.Id, true);
            if (fromApi)
            {
                var apiId = await _unitOfWork.OtherApiRepository
                    .GetApiProductIdAsync(product.Id);
                var respo = await _apiService.SendOrderAsync(apiId, dto.Quantity, dto.PlayerId);
                if (!respo.Success)
                {
                    return Ok(new ApiResponse(200, respo.Message));
                }

                _unitOfWork.OtherApiRepository.AddOrder(new ApiOrder
                {
                    Order = order,
                    ApiOrderId = respo.OrderId
                });
            }

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
    [Authorize(Policy = "Required_Normal_Role")]
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

    [Authorize(Policy = "Required_VIP_Role")]
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

            if (order.User.Id != user.Id)
                return BadRequest(new ApiResponse(403));

            if (order.CreatedAt.AddSeconds(60).CompareTo(DateTime.UtcNow) > 0)
                return BadRequest(new ApiResponse(400, "you can cancel this order after " +
                                                       CalcSeconds(order.CreatedAt.AddSeconds(60).Second,
                                                           DateTime.UtcNow.Second) + " seconds"));

            if (order.Status != 0 && order.Status != 5)
                return BadRequest(new ApiResponse(400,
                    "you can't cancel this order because it has been checked by admin"));


            if (order.Status == 5)
                return Ok(new ApiResponse(200, "you can't cancel this order because it has been cancelled"));

            var roles = User.GetRoles();
            var normal = roles.Any(x => x.ToLower() == "normal");

            var fromApi = await _unitOfWork.OtherApiRepository
                .CheckIfOrderExistAsync(order.Id, true);

            if (fromApi)
            {
                if (normal)
                    return Ok(new ApiResponse(200, "can't do this action"));

                var apiId = await _unitOfWork.OtherApiRepository
                    .GetApiOrderIdAsync(order.Id);

                var response = await _apiService.CancelOrderByIdAsync(apiId);

                if (!response.Success)
                    return BadRequest(new ApiResponse(400, response.Message));

                _unitOfWork.OtherApiRepository.DeleteOrder(order.Id);
            }

            if (!normal)
            {
                order.User.Balance += order.TotalPrice;
                order.User.TotalPurchasing -= order.TotalPrice;
                order.User.TotalForVIPLevel -= order.TotalPrice;
                order.User.VIPLevel = await _unitOfWork.VipLevelRepository
                    .GetVipLevelForPurchasingAsync(order.User.TotalForVIPLevel);
            }

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