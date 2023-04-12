using AutoMapper;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Extentions;
using ChargingApp.Helpers;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChargingApp.Controllers;


public class AdminAccountController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<AppUser> _userManager;
    private readonly IMapper _mapper;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ITokenService _tokenService;

    public AdminAccountController(IUnitOfWork unitOfWork, UserManager<AppUser> userManager,
        IMapper mapper, SignInManager<AppUser> signInManager, ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _mapper = mapper;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }
    [Authorize(Policy = "Required_Administrator_Role")]
    
    [ProducesResponseType(typeof(ApiOkResponse<AdminDto>), StatusCodes.Status200OK)]
    [HttpPost("create-account")]
    public async Task<ActionResult<AdminDto>> CreateAccount(AdminRegisterDto dto)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.Email))
                return BadRequest(new ApiResponse(400, "Email is required"));

            dto.Email = dto.Email.ToLower();

            if (!SomeUsefulFunction.IsValidEmail(email: dto.Email))
                return BadRequest(new ApiResponse(400, "This email isn't valid"));


            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user != null)
            {
                return BadRequest(new ApiResponse(400, "This email is already used"));
            }

            if (dto.Roles.Count == 0)
                return BadRequest(new ApiResponse(400, "Roles shouldn't be empty"));

            if (dto.Roles.Any(x => x.ToLower() == "admin"))
                return BadRequest(new ApiResponse(400, "can't add admin user"));

            user = new AppUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName.ToLower(),
                LastName = dto.LastName.ToLower(),
                Country = dto.Country,
                City = dto.City,
                VIPLevel = 1,
                PhoneNumber = dto.PhoneNumer,
                EmailConfirmed = true
            };
            var res = await _userManager.CreateAsync(user, dto.Password);

            if (res.Succeeded == false) return BadRequest(res.Errors);

            var roleResult = await _userManager.AddToRolesAsync(user, dto.Roles);

            if (!roleResult.Succeeded) return BadRequest(new ApiResponse(400, "Failed to add roles"));

            var admin = _mapper.Map<AdminDto>(user);
            admin.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            return Ok(new ApiOkResponse<AdminDto>(admin));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    [ProducesResponseType(typeof(ApiOkResponse<>), StatusCodes.Status200OK)]

    [HttpPost("login")]
    public async Task<ActionResult<AdminLoginDto>> Login(LoginDto loginDto)
    {
        try
        {
            loginDto.Email = loginDto.Email.ToLower();

            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == loginDto.Email);
            if (user == null) return Unauthorized(new ApiResponse(401, "Invalid Email"));

            var res = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!res.Succeeded) return Unauthorized(new ApiResponse(400, "Invalid password"));

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                return BadRequest(new ApiResponse(400, "Please Confirm your Email"));
            }

            var roles = await _userManager.GetRolesAsync(user);

            if (!SomeUsefulFunction.CheckIfItIsAnAdmin(roles))
                return BadRequest(new ApiResponse(403));
            
            return Ok(new ApiOkResponse<AdminLoginDto>(new AdminLoginDto
            {
                Email = user.Email.ToLower(),
                FirstName = user.FirstName.ToLower(),
                LastName = user.LastName.ToLower(),
                Token = await _tokenService.CreateToken(user),
                PhoneNumber = user.PhoneNumber,
                City = user.City,
                Country = user.Country,
                Roles = roles.ToList()
            }));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpGet("info")]
    public async Task<ActionResult<AdminDto>> AdminInfo()
    {
        var email = User.GetEmail();

        if (string.IsNullOrEmpty(email)) return Unauthorized(new ApiResponse(401));

        var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(email);

        if (user is null) return Unauthorized(new ApiResponse(401));

        var roles = User.GetRoles().ToList();
            
        if (!SomeUsefulFunction.CheckIfItIsAnAdmin(roles))
            return BadRequest(new ApiResponse(403));
        
         
        return Ok(new ApiOkResponse<AdminDto>(new AdminDto
        {
            Email = user.Email.ToLower(),
            FirstName = user.FirstName.ToLower(),
            LastName = user.LastName.ToLower(),
            PhoneNumber = user.PhoneNumber,
            City = user.City,
            Country = user.Country,
            Roles = roles.ToList()
        }));
        
        
    }
    [Authorize(Policy = "Required_Administrator_Role")]
    [HttpDelete("email")]
    public async Task<ActionResult<bool>> DeleteAccount([FromQuery] string email)
    {
        if (string.IsNullOrEmpty(email))
            return BadRequest(new ApiResponse(400));

        email = email.ToLower();
        var isValidEmail = SomeUsefulFunction.IsValidEmail(email);

        if (!isValidEmail)
            return BadRequest(new ApiResponse(400, "You should provide a valid email"));

        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
            return BadRequest(new ApiResponse(404, "User with email : " + email + " not found"));
        var roles = await _userManager.GetRolesAsync(user);

        if (roles.Any(x => x.ToLower() == "admin"))
            return BadRequest(new ApiResponse(400, "You can't delete the user with an admin role"));

        if (!roles.Any(x => x.ToLower() != "normal" && x.ToLower() != "vip"))
            return BadRequest(new ApiResponse(400, "You can't delete this user"));

        var orders = await _unitOfWork.OrdersRepository
            .GetOrdersForSpecificProduct(user.Id);

        foreach (var t in orders.Where(t => t != null))
        {
            _unitOfWork.OrdersRepository.DeleteOrder(t);
        }

        var rechargeCodes = await _unitOfWork.RechargeCodeRepository
            .GetCodesForUserAsync(user.Id);

        foreach (var code in rechargeCodes)
        {
            code.User = null;
            _unitOfWork.RechargeCodeRepository.Update(code);
        }

        _unitOfWork.UserRepository.DeleteUser(user);

        if (!await _unitOfWork.Complete())
            return BadRequest(new ApiResponse(400, "Can't delete the user... something went wrong"));

        await _userManager.DeleteAsync(user);
        return Ok(new ApiResponse(200, "User deleted"));
    }
}