using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Extentions;
using ChargingApp.Helpers;
using ChargingApp.Interfaces;
using ChargingApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChargingApp.Controllers;

public class AccountController : BaseApiController
{
    private readonly IEmailHelper _emailSender;
    private readonly ITokenService _tokenService;
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IUnitOfWork _unitOfWork;

    public AccountController(IEmailHelper emailSender, ITokenService tokenService, UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager, IUnitOfWork unitOfWork)
    {
        _emailSender = emailSender;
        _tokenService = tokenService;
        _userManager = userManager;
        _signInManager = signInManager;
        _unitOfWork = unitOfWork;
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterDto registerDto)
    {
        try
        {
            registerDto.Email = registerDto.Email.ToLower();

            var user = await _userManager.FindByEmailAsync(registerDto.Email);
            if (user != null)
            {
                if (user.EmailConfirmed)
                    return BadRequest(new ApiResponse(400, "This email is already used","هذا الحساب مستخدم من قبل"));
                var response = await GenerateTokenAndSendEmailForUser(user);

                if (!response)
                    return BadRequest(new ApiResponse(400, "Failed to send email."));

                return Ok(new ApiResponse(200, "You have already registered with this Email," +
                                               "The confirmation link will be resent to your email," +
                                               " please check your email and confirm your account.",
                    "انت مسجل مسبقا بهذا الحساب, سيتم إعادة إرسال رابط التأكيد إليك... الرجاء التأكد من صندوق الوارد لديك من اجل تاكيد حسابك"));
            }

            if (!SomeUsefulFunction.IsValidEmail(registerDto.Email))
                return BadRequest(new ApiResponse(400,"Wrong email","بريد إالكتروني خاطئ"));
            
            registerDto.AccountType = registerDto.AccountType.ToLower();
            
            if (registerDto.AccountType != "normal" && registerDto.AccountType != "vip")
                return BadRequest(new ApiResponse(400, "account type should be normal or vip"));

            user = new AppUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName.ToLower(),
                LastName = registerDto.LastName.ToLower(),
                Country = registerDto.Country,
                City = registerDto.City,
                VIPLevel = (registerDto.AccountType == "normal" ? 0 : 1),
                PhoneNumber = registerDto.PhoneNumer
            };
            var res = await _userManager.CreateAsync(user, registerDto.Password);

            if (res.Succeeded == false) return BadRequest(res.Errors);

            var respons = await GenerateTokenAndSendEmailForUser(user);

            IdentityResult roleResult;
            if (registerDto.AccountType == "normal")
                roleResult = await _userManager.AddToRoleAsync(user, "Normal");
            else roleResult = await _userManager.AddToRoleAsync(user, "VIP");


            if (!roleResult.Succeeded) return BadRequest(new ApiResponse(400, "Failed to add roles"));
            if (!respons)
                return BadRequest(new ApiResponse(400, "Failed to send email.","حدثت مشكلة اثناء إرسال رسالة التأكيد... يرجى المحاولة لاحقا"));

            return Ok(new ApiResponse(200, "The confirmation link was send to your email successfully, " +
                                           "please check your email and confirm your account.",
                "تم إرسال رابط التأكيد إلى البريد الإلكتروني الخاص بك, الرجاء التأمد من صندوق الوارد لديك وتأكيد حسابك."));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private async Task<bool> GenerateTokenAndSendEmailForUser(AppUser user)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        var confirmationLink = Url.Action("ConfirmEmail", "Account",
            new { userId = user.Id, token }, Request.Scheme);

        var text = "<html><body>To confirm your email please<a href=" + confirmationLink +
                   "> click here</a></body></html>";
        return await _emailSender.SendEmailAsync(user.Email, "Confirmation Email", text);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        try
        {
            loginDto.Email = loginDto.Email.ToLower();

            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == loginDto.Email);
            if (user == null) return Unauthorized(new ApiResponse(401, "Invalid Email"));

            var res = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!res.Succeeded) return Unauthorized(new ApiResponse(400, "Invalid password"));
            
            var roles = await _userManager.GetRolesAsync(user);


            if (SomeUsefulFunction.CheckIfItIsAnAdmin(roles))
                return BadRequest(new ApiResponse(403,"You can't login with an admin account"));

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                return BadRequest(new ApiResponse(400, "Please Confirm your Email","الرجاء تأكيد حسابك أولا"));
            }

            if (roles.Any(x => x.ToLower() == "vip"))
            {
                var syrian = await _unitOfWork.CurrencyRepository.GetSyrianCurrency();
                var turkish = await _unitOfWork.CurrencyRepository.GetTurkishCurrency();
                var vipPurchase = user.TotalForVIPLevel;
                vipPurchase -= await _unitOfWork.VipLevelRepository
                    .GetMinimumPurchasingForVipLevelAsync(user.VIPLevel);

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
                if (user.Debit > 0)
                {
                    myWallet.TurkishBalance *= -1;
                    myWallet.SyrianBalance *= -1;
                    myWallet.DollarBalance *= -1;
                }

                return Ok(new ApiOkResponse(new UserDto
                {
                    Email = user.Email.ToLower(),
                    FirstName = user.FirstName.ToLower(),
                    LastName = user.LastName.ToLower(),
                    Token = await _tokenService.CreateToken(user),
                    PhoneNumber = user.PhoneNumber,
                    City = user.City,
                    Country = user.Country,
                    MyWallet = myWallet,
                    AccountType = "Vip " + (user.VIPLevel)
                }));
            }

            var normalUser = new NormalUserDto
            {
                Email = user.Email.ToLower(),
                FirstName = user.FirstName.ToLower(),
                LastName = user.LastName.ToLower(),
                Token = await _tokenService.CreateToken(user),
                PhoneNumber = user.PhoneNumber,
                City = user.City,
                Country = user.Country,
                AccountType = "Normal"
            };
            return Ok(new ApiOkResponse(normalUser));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpGet("confirm-email")]
    [AllowAnonymous]
    public async Task<ActionResult> ConfirmEmail(string? userId, string? token)
    {
        if (userId is null || token is null)
        {
            return BadRequest(new { message = "token or userId missing" });
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return BadRequest(new ApiResponse(400, "invalid token user id"));
        }

        var res = await _userManager.ConfirmEmailAsync(user, token);

        if (!res.Succeeded) return BadRequest(new ApiResponse(400, "confirmation failed"));

        return Ok("Your Email is Confirmed try to login in now");
    }

    [HttpPost("forget-password")]
    [AllowAnonymous]
    public async Task<ActionResult> ForgetPassword(UpdateEmailDto dto)
    {
        try
        {
            var email = dto.Email?.ToLower();
            if (email is null) return BadRequest(new ApiResponse(400, "email must be provided"));
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return Unauthorized(new ApiResponse(401, "user was not found"));
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var code = ConfirmationCodesService.GenerateCode(user.Id, token);

            var confirmationLink = Url.Action("ResetPassword", "Account",
                new { userId = user.Id, token = token }, Request.Scheme);

            var text = "<html><body> The code to reset your password is : " + code +
                       "</body></html>";
            var res = await _emailSender.SendEmailAsync(user.Email, "Reset Password", text);

            if (!res)
                return BadRequest(new ApiResponse(400, "Failed to send email."));

            return Ok(new ApiResponse(200, "The Code was sent to your email"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult> ResetPassword(RestDto restDto)
    {
        try
        {
            var newPassword = restDto.NewPassword;
            var code = restDto.Code;
            if (newPassword == null || code == null)
                return BadRequest(new ApiResponse(400, "The password should not be empty"));
            var val = ConfirmationCodesService.GetUserIdAndToken(code);

            if (val is null)
                return BadRequest(new ApiResponse(400, "the code is incorrect","الكود خاطئ, يرجى التأكد من الكود والمحاولة لاحقا"));
            
            var userId = val.Value.userId;
            var token = val.Value.token;

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return Unauthorized(new ApiResponse(401, "this user is not registered"));

            var res = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (res.Succeeded == false) return BadRequest(new ApiResponse(400, "Cannot reset password"));

            ConfirmationCodesService.RemoveUserCodes(userId);
            return Ok(new ApiResponse(200, "Password was reset successfully"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}