﻿using AutoMapper;
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

    [Authorize(Policy = "Required_JustNORMAL_Role")]
    [HttpGet("normal-my-order")]
    [ProducesResponseType(typeof(ApiOkResponse<IEnumerable<NormalOrderDto>>), StatusCodes.Status200OK)]

    public async Task<ActionResult<IEnumerable<NormalOrderDto>>> GetMyOrdersNormal()
    {
        try
        {
            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(User.GetEmail());

            if (user is null) return BadRequest(new ApiResponse(401));
            var roles = User.GetRoles().ToList();
            if (SomeUsefulFunction.CheckIfItIsAnAdmin(roles))
                return BadRequest(new ApiResponse(403, "You're an admin... can't make an order with this account"));

            return Ok(new ApiOkResponse<IEnumerable<NormalOrderDto>>
                (await _unitOfWork.OrdersRepository.GetNormalUserOrdersAsync(user.Id)));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [Authorize(Policy = "Required_JustVIP_Role")]
    [HttpGet("vip-my-order")]
    [ProducesResponseType(typeof(ApiOkResponse<IEnumerable<OrderDto>>), StatusCodes.Status200OK)]

    public async Task<ActionResult<IEnumerable<OrderDto>>> GetMyOrdersVip()
    {
        try
        {
            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(User.GetEmail());

            if (user is null) return BadRequest(new ApiResponse(401));

            var roles = User.GetRoles().ToList();
            if (SomeUsefulFunction.CheckIfItIsAnAdmin(roles))
                return BadRequest(new ApiResponse(403, "You're an admin... can't make an order with this account"));

            return Ok(new ApiOkResponse<List<OrderDto>>(await _unitOfWork.OrdersRepository.GetVipUserOrdersAsync(user.Id)));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [Authorize(Policy = "Required_JustVIP_Role")]
    [HttpPost("vip-order")]
    [ProducesResponseType(typeof(ApiOkResponse<OrderDto>), StatusCodes.Status200OK)]

    public async Task<ActionResult> PlaceOrderVip([FromBody] NewOrderDto dto)
    {
        try
        {
            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(User.GetEmail());
            if (user is null) return BadRequest(new ApiResponse(401));
            var roles = User.GetRoles().ToList();
            if (SomeUsefulFunction.CheckIfItIsAnAdmin(roles))
                return BadRequest(new ApiResponse(403, "You're an admin... can't make an order with this account"));

            var product = await _unitOfWork.ProductRepository.GetProductByIdAsync(dto.ProductId);

            if (product is null)
                return BadRequest(new ApiResponse(404, "the product is not found"));

            if (!CheckIfAvailable(product))
                return BadRequest(new ApiResponse(404, "this product is not available now"));

            if (dto.PlayerName.Length == 0)
                return BadRequest(new ApiResponse(400, "player name is required"));

            if (dto.Quantity < product.MinimumQuantityAllowed)
                return BadRequest(new ApiResponse(400,
                    "the minimum quantity you can chose is " + product.MinimumQuantityAllowed
                    ,product.MinimumQuantityAllowed+"يجب ان تكون الكمية اكبر او يساوي "));

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

            var lastVipLevel = user.VIPLevel;

            if (!product.CanChooseQuantity)
            {
                order.TotalPrice = await PriceForVIP.CannotChooseQuantity((int)dto.Quantity,
                    product,
                    user,
                    _unitOfWork);
                order.TotalQuantity = dto.Quantity;
            }
            else
            {
                order.TotalQuantity = await
                    PriceForVIP.CalcTotalQuantity(product.Quantity, product, user, _unitOfWork);

                var specificPrice = await _unitOfWork.SpecificPriceForUserRepository
                    .GetProductPriceForUserAsync(product.Id, user);

                if (specificPrice != null)
                {
                    order.TotalQuantity = product.Quantity;
                    order.TotalPrice = (decimal)specificPrice;
                }
                else
                    order.TotalPrice = product.Price;

                order.Quantity = product.Quantity;
                user.TotalPurchasing += order.TotalPrice;
                user.TotalForVIPLevel += order.TotalPrice;
            }

            if (order.TotalPrice > user.Balance)
                return BadRequest(new ApiResponse(400, "you have no enough money to do this","ليس لديك رصيد كاف"));

            var fromApi = await _unitOfWork.OtherApiRepository
                .CheckIfProductExistAsync(product.Id, true);
            if (fromApi)
            {
                var apiProduct = await _unitOfWork.OtherApiRepository
                    .GetProductByOurIdAsync(product.Id);
                var hostingSite = apiProduct.HostingSite;

                var apiId = await _unitOfWork.OtherApiRepository
                    .GetApiProductIdAsync(product.Id);
                var respo = await _apiService.SendOrderAsync(apiId, dto.Quantity,
                    dto.PlayerId, hostingSite.BaseUrl, hostingSite.Token);
                if (!respo.Success)
                {
                    return BadRequest(new ApiResponse(400, respo.Message));
                }

                _unitOfWork.OtherApiRepository.AddOrder(new ApiOrder
                {
                    Order = order,
                    ApiOrderId = respo.OrderId,
                    HostingSite = hostingSite
                });
            }

            user.Balance -= order.TotalPrice;
            order.User.VIPLevel = await _unitOfWork.VipLevelRepository
                .GetVipLevelForPurchasingAsync(order.User.TotalForVIPLevel);
            var lvl = await _unitOfWork.VipLevelRepository
                .GetVipLevelAsync(user.VIPLevel);

            if (lastVipLevel < user.VIPLevel)
            {
                var curr = new NotificationHistory
                {
                    User = order.User,
                    ArabicDetails = " تم ترقية مستواك الى  " + lvl.ArabicName,
                    EnglishDetails = "Your level has been upgraded to " + lvl.EnglishName
                };
                _unitOfWork.NotificationRepository.AddNotificationForHistoryAsync(curr);

                await _notificationService.VipLevelNotification(order.User.Email,
                    "Vip level status notification",
                    SomeUsefulFunction.GetVipLevelNotification(order.User.VIPLevel));
            }

            _unitOfWork.UserRepository.UpdateUserInfo(user);
            _unitOfWork.OrdersRepository.AddOrder(order);

            if (!await _unitOfWork.Complete()) return BadRequest(new ApiResponse(400, "Something went wrong"));
            var res = _mapper.Map<OrderDto>(order);

            return Ok(new ApiOkResponse<OrderDto>(res));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    // new order 
    [Authorize(Policy = "Required_JustNORMAL_Role")]
    [HttpPost("normal-order")]
    [ProducesResponseType(typeof(ApiOkResponse<NormalOrderDto>), StatusCodes.Status200OK)]

    public async Task<IActionResult> PlaceOrder([FromForm] NewNormalOrderDto dto)
    {
        try
        {
            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(User.GetEmail());

            if (user is null) return BadRequest(new ApiResponse(401));

            var roles = User.GetRoles().ToList();
            if (SomeUsefulFunction.CheckIfItIsAnAdmin(roles))
                return BadRequest(new ApiResponse(403, "You're an admin... can't make an order with this account"));

            var product = await _unitOfWork.ProductRepository.GetProductByIdAsync(dto.ProductId);

            if (product is null)
                return BadRequest(new ApiResponse(404, "the product is not found"));

            if (!CheckIfAvailable(product))
                return BadRequest(new ApiResponse(404, "this product is not available now"));

            var paymentGateway =
                await _unitOfWork.PaymentGatewayRepository.GetPaymentGatewayByNameAsync(dto.PaymentGateway);

            if (paymentGateway is null)
                return BadRequest(new ApiResponse(404, "payment gateway isn't exist"));

            if (string.IsNullOrEmpty(dto.PlayerName))
                return BadRequest(new ApiResponse(400, "player name is required"));

            if (dto.Quantity < product.MinimumQuantityAllowed)
                return BadRequest(new ApiResponse(400,
                    "the minimum quantity you can chose is " + product.MinimumQuantityAllowed
                    ,product.MinimumQuantityAllowed+"يجب ان تكون الكمية اكبر او يساوي "));

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
                    PriceForNormal.CannotChooseQuantity(
                        dto.Quantity, product, user, _unitOfWork);
                order.TotalQuantity = dto.Quantity;
            }
            else
            {
                order.TotalQuantity = await
                    PriceForNormal.CalcTotalQuantity(product.Quantity, product, user, _unitOfWork);

                order.TotalPrice = product.Price;
                order.Quantity = product.Quantity;
            }

            _unitOfWork.OrdersRepository.AddOrder(order);

            if (!await _unitOfWork.Complete())
                return BadRequest(new ApiResponse(400, "Something went wrong"));

            var res = _mapper.Map<NormalOrderDto>(order);
            return Ok(new ApiOkResponse<NormalOrderDto>(res));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [Authorize(Policy = "Required_JustVIP_Role")]
    [HttpDelete]
    public async Task<ActionResult> DeleteOrder(int orderId)
    {
        try
        {
            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(User.GetEmail());
            if (user is null) return Unauthorized(new ApiResponse(401));
            var rols = User.GetRoles().ToList();
            if (SomeUsefulFunction.CheckIfItIsAnAdmin(rols))
                return BadRequest(new ApiResponse(403, "You're an admin... can't make an order with this account"));

            var order = await _unitOfWork.OrdersRepository.GetOrderByIdAsync(orderId);

            if (order is null)
                return BadRequest(new ApiResponse(400, "this order isn't exist"));

            if (order.User.Id != user.Id)
                return BadRequest(new ApiResponse(403));

            if (order.CreatedAt.AddSeconds(60).CompareTo(DateTime.UtcNow) > 0)
            {
                var rem = CalcSeconds(order.CreatedAt.AddSeconds(60).Second,
                    DateTime.UtcNow.Second);
                return BadRequest(new ApiResponse(400, "you can cancel this order after " +
                                                       rem + " seconds"
                    , " ثانية"+rem + "يمكنك إلغاء الطلب بعد "));
            }

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
                    return Ok(new ApiResponse(200, "can't do this action","لا يمكنك إلغاء الطلب"));

                var apiId = await _unitOfWork.OtherApiRepository
                    .GetApiOrderIdAsync(order.Id);
                var apiOrder = await _unitOfWork.OtherApiRepository
                    .GetOrderByOurIdAsync(orderId);

                var hostingSite = apiOrder.HostingSite;
                var response = await _apiService.CancelOrderByIdAsync(apiId,
                    hostingSite.BaseUrl, hostingSite.Token);

                if (!response.Success)
                    return BadRequest(new ApiResponse(400, response.Message));

                _unitOfWork.OtherApiRepository.DeleteOrder(order.Id);
            }

            var lastVip = user.VIPLevel;
            if (!normal)
            {
                order.User.Balance += order.TotalPrice;
                order.User.TotalPurchasing -= order.TotalPrice;
                order.User.TotalForVIPLevel -= order.TotalPrice;
                order.User.VIPLevel = await _unitOfWork.VipLevelRepository
                    .GetVipLevelForPurchasingAsync(order.User.TotalForVIPLevel);
            }

            var lvl = await _unitOfWork.VipLevelRepository
                .GetVipLevelAsync(order.User.VIPLevel);
            
            _unitOfWork.NotificationRepository.AddNotificationForHistoryAsync(
                new NotificationHistory
                {
                    User = order.User,
                    ArabicDetails = " تم الغاء الطلب رقم " + orderId,
                    EnglishDetails = "Order with id " + orderId + " has been cancelled "
                });
            if (lastVip > order.User.VIPLevel)
            {
                var curr = new NotificationHistory
                {
                    User = order.User,
                    ArabicDetails = " تم اعادة مستواك الى  " + lvl.ArabicName,
                    EnglishDetails = "Your level has been returned back to " + lvl.EnglishName
                };
                _unitOfWork.NotificationRepository.AddNotificationForHistoryAsync(curr);
            }

            order.Status = 5;
            order.StatusIfCanceled = 2;

            var not = new OrderAndPaymentNotification()
            {
                User = order.User,
                Order = order
            };


            if (!await _unitOfWork.Complete())
                return BadRequest(new ApiResponse(400, "Something went wrong during deleting the order","حدثت مشكلة اثناء حذف الطلب"));

            if (lastVip > user.VIPLevel)
                await _notificationService.VipLevelNotification(order.User.Email,
                    "Vip level status notification",
                    SomeUsefulFunction.GetVipLevelNotification(user.VIPLevel));

            await _notificationService.NotifyUserByEmail(order.User.Email, _unitOfWork, not,
                "Order status notification", SomeUsefulFunction.GetOrderNotificationDetails(order));

            return Ok(new ApiResponse(201, "Cancelled successfully "));
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