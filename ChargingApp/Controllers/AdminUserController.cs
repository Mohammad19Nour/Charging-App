using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

[Authorize(Policy = "Required_Administrator_Role")]
public class AdminUserController : AdminController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<AppUser> _userManager;

    public AdminUserController(IUnitOfWork unitOfWork, UserManager<AppUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    [HttpPost("update-user-vip-level")]
    public async Task<IActionResult> UpdateVipLevelForUser(string userEmail, int newVipLevel)
    {
        try
        {
            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(userEmail);

            if (user is null) return NotFound(new ApiResponse(401, "user not fount"));

            var roles = await _userManager.GetRolesAsync(user);
            var tmp = roles.Any(x => x.ToLower() == "normal");

            if (tmp)
                return BadRequest(new ApiResponse(400, "can't update normal account"));

            var vipLevel = await _unitOfWork.VipLevelRepository.CheckIfExist(newVipLevel);

            if (!vipLevel) return NotFound(new ApiResponse(401, "vip level not fount"));

            var havePendingOrders = await _unitOfWork.OrdersRepository
                .CheckPendingOrdersForUserByEmailAsync(userEmail);

            if (havePendingOrders)
                return BadRequest(new ApiResponse(400,
                    "this user have pending orders. please check the order/s and try again"));

            if (newVipLevel < user.VIPLevel)
                return BadRequest(new ApiResponse(400, "can't downgrade vip level"));

            user.VIPLevel = newVipLevel;
            user.TotalForVIPLevel = await _unitOfWork.VipLevelRepository
                .GetMinimumPurchasingForVipLevelAsync(newVipLevel);

            _unitOfWork.UserRepository.UpdateUserInfo(user);
            
            if (await _unitOfWork.Complete())
                return Ok(new ApiResponse(200, "updated successfully"));

            return BadRequest(new ApiResponse(400, "Failed to update user vip level"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpPost("add-debit")]
    public async Task<ActionResult> AddDebit(string userEmail, double debitValue)
    {
        try
        {
            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(userEmail);

            if (user is null)
                return NotFound(new ApiResponse(404, "user not found"));

            if (debitValue <= 0)
                return BadRequest(new ApiResponse(400, "value should be greater than 0"));

            var roles = await _userManager.GetRolesAsync(user);
            var tmp = roles.Any(x => x.ToLower() == "normal");

            if (tmp)
                return BadRequest(new ApiResponse(400, "can't add to normal account"));


            user.Balance += debitValue;
            user.Debit += debitValue;

            _unitOfWork.UserRepository.UpdateUserInfo(user);
            _unitOfWork.DebitRepository.AddDebit(new DebitHistory
            {
                User = user,
                DebitValue = debitValue
            });
            
            if (await _unitOfWork.Complete())
                return Ok(new ApiResponse(200, "added successfully"));

            return BadRequest(new ApiResponse(400, "Failed tp add debit"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpPost("add-specific-price")]
    public async Task<ActionResult> AddSpecificPriceForUser([FromBody] NewSpecificPriceForUserDto dto)
    {
        try
        {
            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(dto.Email);
            if (user is null)
                return NotFound(new ApiResponse(404, "user not found"));

            if (user.VIPLevel == 0)
                return BadRequest(new ApiResponse(403, "Can't add for normal user"));

            var product = await _unitOfWork.ProductRepository.GetProductByIdAsync(dto.ProductId);

            if (product is null)
                return NotFound(new ApiResponse(404, "product not found"));

            var vipLevel = await _unitOfWork.VipLevelRepository.CheckIfExist(dto.VipLevel);

            if (!vipLevel)
                return NotFound(new ApiResponse(404, "vip level not found"));

            var tmp = await _unitOfWork.SpecificPriceForUserRepository
                .GetPriceForUserAsync(dto.Email.ToLower(), dto.VipLevel, dto.ProductId);

            if (tmp != null) return await UpdateSpecificPriceForUser(dto);

            var spec = new SpecificPriceForUser
            {
                ProductId = dto.ProductId,
                User = user,
                ProductPrice = dto.ProductPrice,
                VipLevel = dto.VipLevel
            };
            _unitOfWork.SpecificPriceForUserRepository.AddProductPriceForUser(spec);

            _unitOfWork.UserRepository.UpdateUserInfo(user);
            if (await _unitOfWork.Complete())
                return Ok(new ApiResponse(200, "added successfully"));

            return BadRequest(new ApiResponse(400, "Failed to add price for user"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpPost("update-specific-price")]
    public async Task<ActionResult> UpdateSpecificPriceForUser([FromBody] NewSpecificPriceForUserDto dto)
    {
        try
        {
            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(dto.Email);
            if (user is null)
                return NotFound(new ApiResponse(404, "user not found"));
            if (user.VIPLevel == 0)
                return BadRequest(new ApiResponse(403, "Can't update for normal user"));
            var product = await _unitOfWork.ProductRepository.GetProductByIdAsync(dto.ProductId);

            if (product is null)
                return NotFound(new ApiResponse(404, "product not found"));

            var vipLevel = await _unitOfWork.VipLevelRepository.CheckIfExist(dto.VipLevel);

            if (!vipLevel)
                return NotFound(new ApiResponse(404, "vip level not found"));

            var spec = await _unitOfWork.SpecificPriceForUserRepository
                .GetPriceForUserAsync(dto.Email, dto.VipLevel, dto.ProductId);

            if (spec is null)
                return NotFound(new ApiResponse(404, "the specific price is not exist"));

            spec.ProductPrice = dto.ProductPrice;
            _unitOfWork.SpecificPriceForUserRepository.UpdateProductPriceForUser(spec);

            if (await _unitOfWork.Complete())
                return Ok(new ApiResponse(200, "Updated successfully"));

            return BadRequest(new ApiResponse(400, "Failed to Updated price for user"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}