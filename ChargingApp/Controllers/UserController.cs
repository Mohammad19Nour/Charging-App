using AutoMapper;
using ChargingApp.Entity;
using ChargingApp.Extentions;
using ChargingApp.Data;
using ChargingApp.DTOs;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

[Authorize]
public class UserController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly UserManager<AppUser> _userManager;

    public UserController(IUnitOfWork unitOfWork, IMapper mapper,
        UserManager<AppUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _userManager = userManager;
    }

    [HttpPut("update-user-info")]
    public async Task<ActionResult> UpdateUser(UpdateUserInfoDto updateUserInfoDto)
    {
        var email = User.GetEmail();
        var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(email);

        _mapper.Map(updateUserInfoDto, user);
        _unitOfWork.UserRepository.UpdateUserInfo(user);

        if (await _unitOfWork.Complete()) return Ok(new ApiResponse(200, "Updated"));
        return BadRequest(new ApiResponse(400, "Failed to update"));
    }

    [HttpGet("user-info")]
    public async Task<ActionResult<UserInfoDto>> GetUserInfo()
    {
        var email = User.GetEmail();
        
        if (email is null) return Unauthorized(new ApiResponse(401));
        
        var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(email);
        
        if (user is null) return Unauthorized(new ApiResponse(401));
        
        var res = _mapper.Map<UserInfoDto>(user);
        
        var syrian = await _unitOfWork.CurrencyRepository.GetSyrianCurrency();
        var turkish = await _unitOfWork.CurrencyRepository.GetTurkishCurrency();

        res.MyWallet = new WalletDto
        {
            DollarBalance = user.Balance,
            SyrianBalance = user.Balance * syrian,
            TurkishBalance = user.Balance * turkish,
            
            DollarDebit = user.Debit,
            SyrianDebit = user.Debit * syrian,
            TurkishDebit = user.Debit * turkish,
            
            
            DollarTotalPurchase = user.TotalPurchasing,
            SyrianTotalPurchase = user.TotalPurchasing * syrian,
            TurkishTotalPurchase = user.TotalPurchasing * turkish,
        };
        if (user.Debit > 0)
        {
            res.MyWallet.TurkishBalance *= -1;
            res.MyWallet.SyrianBalance *= -1;
            res.MyWallet.DollarBalance *= -1;
        }

        return Ok(new ApiOkResponse(res));
    }

    [HttpPut("change-password")]
    public async Task<ActionResult> UpdatePassword([FromBody] UpdatePasswordDto dto)
    {
        var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(User.GetEmail());
        if (user is null)
            return Unauthorized(new ApiResponse(403));
        var res = 
            await _userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);

        
        if (!res.Succeeded)
            return BadRequest(new ApiResponse(400, "Failed to update password"));

        return Ok(new ApiResponse(200, "updated successfully"));
    }

    [HttpGet("my-wallet")]
    public async Task<ActionResult<WalletDto>> MyWallet()
    {
        var email = User.GetEmail();

        if (email is null) return Unauthorized(new ApiResponse(401));

        var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(email);
        
        if (user is null) return Unauthorized(new ApiResponse(401));

        try
        {
            var syrian = await _unitOfWork.CurrencyRepository.GetSyrianCurrency();
            var turkish = await _unitOfWork.CurrencyRepository.GetTurkishCurrency();
        
            var myWallet = new WalletDto
            {
                DollarBalance = user.Balance,
                SyrianBalance = user.Balance * syrian,
                TurkishBalance = user.Balance * turkish,
            
                DollarDebit = user.Debit,
                SyrianDebit = user.Debit * syrian,
                TurkishDebit = user.Debit * turkish,
            
            
                DollarTotalPurchase = user.TotalPurchasing,
                SyrianTotalPurchase = user.TotalPurchasing * syrian,
                TurkishTotalPurchase = user.TotalPurchasing * turkish,
            };

            if (user.Debit > 0)
            {
                myWallet.TurkishBalance *= -1;
                myWallet.SyrianBalance *= -1;
                myWallet.DollarBalance *= -1;
            }
            return Ok(new ApiOkResponse(myWallet));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(new ApiResponse(400, "some exception happened"));
            
        }
        
    }
    [HttpGet("account-type")]
    public async Task<ActionResult<string>> AccountType()
    {
        var email = User.GetEmail();

        if (email is null) return Unauthorized(new ApiResponse(401));

        var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(email);
        
        if (user is null) return Unauthorized(new ApiResponse(401));

        var type = "";
        if (user.VIPLevel == 0) type = "Normal";
        else type = "VIP " + user.VIPLevel.ToString();
            return Ok(new ApiOkResponse(type));

    }
}