using AutoMapper;
using ChargingApp.Entity;
using ChargingApp.Extentions;
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
        try
        {
            var email = User.GetEmail();

            if (email is null) return Unauthorized(new ApiResponse(403, "user not fount"));
            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(email);

            if (user is null) return Unauthorized(new ApiResponse(403, "user not fount"));

            _mapper.Map(updateUserInfoDto, user);
            _unitOfWork.UserRepository.UpdateUserInfo(user);

            if (await _unitOfWork.Complete()) return Ok(new ApiResponse(200, "Updated"));
            return BadRequest(new ApiResponse(400, "Failed to update"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpGet("user-info")]
    public async Task<ActionResult<UserInfoDto>> GetUserInfo()
    {
        try
        {
            var email = User.GetEmail();

            if (email is null) return Unauthorized(new ApiResponse(401));

            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(email);

            if (user is null) return Unauthorized(new ApiResponse(401));

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Any(x => x.ToLower() == "vip"))
            {
                var res = _mapper.Map<UserInfoDto>(user);

                var syrian = await _unitOfWork.CurrencyRepository.GetSyrianCurrency();
                var turkish = await _unitOfWork.CurrencyRepository.GetTurkishCurrency();
                var vipPurchase = user.TotalForVIPLevel;
                vipPurchase -= await _unitOfWork.VipLevelRepository
                    .GetMinimumPurchasingForVipLevelAsync(user.VIPLevel);

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

                    TurkishVIPPurchase = vipPurchase * turkish,
                    SurianVIPPurchase = vipPurchase * syrian,
                    DollarVIPPurchase = vipPurchase
                };
                if (!(user.Debit > 0)) return Ok(new ApiOkResponse(res));

                res.MyWallet.TurkishBalance *= -1;
                res.MyWallet.SyrianBalance *= -1;
                res.MyWallet.DollarBalance *= -1;

                return Ok(new ApiOkResponse(res));
            }

            return Ok(new ApiOkResponse(_mapper.Map<NormalUserInfoDto>(user)));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpPut("change-password")]
    public async Task<ActionResult> UpdatePassword([FromBody] UpdatePasswordDto dto)
    {
        try
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
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
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
            var myWallet = await GetMyWallet(user);
            var roles = User.GetRoles();

            var role = roles.FirstOrDefault(x => x.ToLower() == "vip");

            if (role != null)
                return Ok(new ApiOkResponse(myWallet));

            return Ok(new ApiOkResponse(new
                { myWallet.DollarTotalPurchase, myWallet.SyrianTotalPurchase, myWallet.TurkishTotalPurchase }));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [Authorize(Policy = "Required_VIP_Role")]
    [HttpGet("account-info")]
    public async Task<ActionResult> AccountInfo()
    {
        try
        {
            var email = User.GetEmail();

            if (email is null) return Unauthorized(new ApiResponse(401));

            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(email);

            if (user is null) return Unauthorized(new ApiResponse(401));

            var roles = User.GetRoles();
            var role = roles.FirstOrDefault(x => x.ToLower() == "vip");

            if (role is null) return Ok(new ApiOkResponse("Normal"));

            var levels = await _unitOfWork.VipLevelRepository.GetAllVipLevelsAsync();
            levels = levels.Where(x => x.VipLevel != 0).ToList();

            var vipLevels = levels.Select(x => _mapper.Map<VipLevelDto>(x)).ToList();

            decimal percentage = user.TotalForVIPLevel;
            foreach (var lvl in vipLevels)
            {
                if (lvl.VipLevel >= user.VIPLevel) break;
                percentage -= lvl.Purchase;
            }

            percentage = percentage / vipLevels[user.VIPLevel - 1].Purchase * 100;
            var res = new AccountInfoDto
            {
                UserVipLevel = user.VIPLevel,
                PurchasingPercentForCurrentVipLevel = percentage,
                VipLevels = vipLevels
            };

            return Ok(new ApiOkResponse(res));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }


    private async Task<WalletDto> GetMyWallet(AppUser user)
    {
        var syrian = await _unitOfWork.CurrencyRepository.GetSyrianCurrency();
        var turkish = await _unitOfWork.CurrencyRepository.GetTurkishCurrency();

        var vipPurchase = user.TotalForVIPLevel;

        var t = await _unitOfWork.VipLevelRepository
            .GetMinimumPurchasingForVipLevelAsync(user.VIPLevel);
        vipPurchase -= t;
        Console.WriteLine(t + "\n\n");
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

            TurkishVIPPurchase = vipPurchase * turkish,
            SurianVIPPurchase = vipPurchase * syrian,
            DollarVIPPurchase = vipPurchase
        };

        if (!(user.Debit > 0)) return myWallet;
        myWallet.TurkishBalance *= -1;
        myWallet.SyrianBalance *= -1;
        myWallet.DollarBalance *= -1;

        return myWallet;
    }
}