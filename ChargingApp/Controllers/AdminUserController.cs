using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

public class AdminUserController : AdminController
{
    private readonly IUnitOfWork _unitOfWork;

    public AdminUserController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpPost("update-user-vip-level")]
    public async Task<IActionResult> UpdateVipLevelForUser([FromBody] string userEmail , int newVipLevel)
    {
        var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(userEmail);

        if (user is null) return NotFound(new ApiResponse(401 , "user not fount"));

        var vipLevel = await _unitOfWork.VipLevelRepository.CheckIfExist(newVipLevel);
        
        if (!vipLevel)return NotFound(new ApiResponse(401 , "vip level not fount"));

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

        if (await _unitOfWork.Complete())
            return Ok(new ApiResponse(200, "updated successfully"));
        
        return BadRequest(new ApiResponse(400, "Failed to update user vip level"));
    }

    [HttpPost("add-debit")]
    public async Task<ActionResult> AddDebit(string userEmail, double debitValue)
    {
        var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(userEmail);

        if (user is null)
            return NotFound(new ApiResponse(404, "user not found"));

        if (debitValue <= 0)
            return BadRequest(new ApiResponse(400, "value should be greater than 0"));

        user.Balance += debitValue;
        user.Debit += debitValue;
        
        _unitOfWork.UserRepository.UpdateUserInfo(user);
        _unitOfWork.DebitRepository.AddDebit(new DebitHistory
        {
            User = user,
            DebitValue = debitValue
        });

        if (await _unitOfWork.Complete())
            return Ok(new ApiResponse(200, "update successfully"));

        return BadRequest(new ApiResponse(400, "Failed tp add debit"));
    }
    
    [HttpPost("add-specific-price")]
    public async Task<ActionResult> AddSpecificPriceForUser([FromBody] NewSpecificPriceForUserDto dto)
    {
        var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(dto.Email);
        if (user is null)
            return NotFound(new ApiResponse(404, "user not found"));
      
        if (user.VIPLevel == 0)
            return BadRequest(new ApiResponse(403,"Can't add for normal user"));

        var product = await _unitOfWork.ProductRepository.GetProductByIdAsync(dto.ProductId);
        
        if (product is null)
            return NotFound(new ApiResponse(404, "product not found"));

        var vipLevel = await _unitOfWork.VipLevelRepository.CheckIfExist(dto.VipLevel);
        
        if (!vipLevel)
            return NotFound(new ApiResponse(404, "vip level not found"));

        var tmp = await _unitOfWork.SpecificPriceForUserRepository
            .GetPriceForUserAsync(dto.Email,dto.VipLevel,dto.ProductId);

        if (tmp != null) return await UpdateSpecificPriceForUser(dto);
        
        var spec = new SpecificPriceForUser
        {
            ProductId = dto.ProductId,
            User = user,
            ProductPrice = dto.ProductPrice,
            VipLevel = dto.VipLevel
        };
        _unitOfWork.SpecificPriceForUserRepository.AddProductPriceForUser(spec);

        if (await _unitOfWork.Complete()) 
            return Ok(new ApiResponse(200, "added successfully"));

        return BadRequest(new ApiResponse(400, "Failed to add price for user"));
    }

    [HttpPost("update-specific-price")]
    public async Task<ActionResult> UpdateSpecificPriceForUser([FromBody] NewSpecificPriceForUserDto dto)
    {
        var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(dto.Email);
        if (user is null)
            return NotFound(new ApiResponse(404, "user not found"));
        if (user.VIPLevel == 0)
            return BadRequest(new ApiResponse(403,"Can't update for normal user"));
        var product = await _unitOfWork.ProductRepository.GetProductByIdAsync(dto.ProductId);
        
        if (product is null)
            return NotFound(new ApiResponse(404, "product not found"));

        var vipLevel = await _unitOfWork.VipLevelRepository.CheckIfExist(dto.VipLevel);
        
        if (!vipLevel)
            return NotFound(new ApiResponse(404, "vip level not found"));

        var spec = await _unitOfWork.SpecificPriceForUserRepository
            .GetPriceForUserAsync(dto.Email,dto.VipLevel,dto.ProductId);

        if (spec is null)
            return NotFound(new ApiResponse(404));
        
        spec.ProductPrice = dto.ProductPrice;
        _unitOfWork.SpecificPriceForUserRepository.UpdateProductPriceForUser(spec);

        if (await _unitOfWork.Complete()) 
            return Ok(new ApiResponse(200, "Updated successfully"));

        return BadRequest(new ApiResponse(400, "Failed to Updated price for user"));
    }
}