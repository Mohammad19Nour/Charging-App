using Charging_App.DTOs;
using Charging_App.Entity;
using Charging_App.Errors;
using Charging_App.Interfaces;
using Charging_App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Charging_App.Controllers;

public class AccountController : BaseApiController
{
    private readonly IEmailSender _emailSender;
    private readonly ITokenService _tokenService;
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;

    public AccountController(IEmailSender emailSender,ITokenService tokenService,UserManager<AppUser> userManager , SignInManager<AppUser> signInManager)
    {
        _emailSender = emailSender;
        _tokenService = tokenService;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        registerDto.Email = registerDto.Email.ToLower();
        var user = new AppUser
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            FirstName = registerDto.FirstName.ToLower(),
            LastName = registerDto.LastName.ToLower(),
            CountryCode = registerDto.CountryCode,
            City = registerDto.City,
            AccountType = registerDto.AccountType
            
        };

        var result = await _userManager.FindByEmailAsync(registerDto.Email);
        if (result != null)
        {
            if (result.EmailConfirmed) return BadRequest(new ApiResponse( 400, "This email was token but not confirmed"));
            return BadRequest(new ApiResponse(400 , "This email is token"));
        }

        var res = await _userManager.CreateAsync(user,registerDto.Password);

        if (res.Succeeded == false) return BadRequest(res.Errors) ;
        
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        var confirmationLink = Url.Action("ConfirmEmail" , "Account", 
            new{userId = user.Id , token = token}, Request.Scheme);

        string text = "<html><body>To confirm your email please<a href=" + confirmationLink +
                      "> click here</a></body></html>";
      await  _emailSender.SendEmailAsync(user.Email , "Confirmation Email",text);
      //  return Ok(confirmationLink + "\n\n" + await _tokenService.CreateToken(user));
       // var roleRes = await _userManager.AddToRoleAsync(user, "Member");

     //   if (roleRes.Succeeded == false) return BadRequest("Happend role");


     return Ok("A confirmation link was sent to your email address, please check your email and confirm your account");
        return new UserDto
        {
            FirstName = registerDto.FirstName.ToLower(),
            LastName = registerDto.LastName.ToLower(),
       Token = await _tokenService.CreateToken(user)
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == loginDto.Email);
        if (user == null) return Unauthorized(new ApiResponse(401 , "Invalid Email"));
        
        if (! await _userManager.IsEmailConfirmedAsync(user))
        {
            return BadRequest(new ApiResponse(400 ,"Please Confirm your Email"));
        }

        var res = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password , false);
        if (!res.Succeeded) return Unauthorized(new ApiResponse(400 , "Invalid password"));

        return new UserDto
        {
            FirstName = user.FirstName.ToLower(),
            LastName = user.LastName.ToLower(),
            Token = await _tokenService.CreateToken(user) 
        };
    }

    [HttpGet("test")]
    public ActionResult<string> testx()
    {
        return "test from here";
    }

    [HttpGet("confirm-email")]
    [AllowAnonymous]
    public async Task<ActionResult> ConfirmEmail(string? userId, string? token)
    {
        if (userId == null || token == null) return BadRequest(new ApiResponse(400 , "yoken or user id is null"));

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return BadRequest(new ApiResponse(400 ,"invalid token user id"));
            
        }
        var res =await _userManager.ConfirmEmailAsync(user, token);

       if (!res.Succeeded) return BadRequest(new ApiResponse(400 ,"confirm fails"));

       return Ok("Your Email is Confirmed try to login in now");
    }

    [HttpGet("forget-password/{userEmail}")]
    [AllowAnonymous]

    public async Task<ActionResult> ForgetPassword(string userEmail)
    {
        userEmail = userEmail.ToLower();
        var user = await _userManager.FindByEmailAsync(userEmail);
        
        if (user == null)
        {
            return BadRequest(new ApiResponse(400 , "user was not found"));
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var code = GenerateRandomeCode.GenerateCode(user.Id, token);
        
        var confirmationLink = Url.Action("ResetPassword" , "Account", 
            new{userId = user.Id , token = token}, Request.Scheme);

        string text = "<html><body> The code to reset your password is : " + code +
                     "</body></html>";
        await  _emailSender.SendEmailAsync(user.Email , "Reset Password",text);

        
        return Ok(text);
    }

    [HttpGet("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult> ResetPassword( string code , string? newPassword)
    {
        if (newPassword == null) 
            return BadRequest(new ApiResponse(400, "The password should not be empty"));
        var val = GenerateRandomeCode.GetUserIdAndToken(code);
        
        if (null == val)
            return BadRequest(new ApiResponse(400 , "the code is incorrect"));
        var userId = val.Keys.First().ToString();
        var token = val[val.Keys.FirstOrDefault()];
        
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return BadRequest(new ApiResponse(400 , "this user is not registered"));

        var res = await _userManager.ResetPasswordAsync(user , token , newPassword);

        if (res.Succeeded == false) return BadRequest(new ApiResponse(400 , "Cannot reset password"));
        
        GenerateRandomeCode.DeleteCode(code);
        return Ok("Password was reset successfully");
    }
}