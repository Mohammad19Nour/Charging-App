using AutoMapper;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Helpers;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

[Authorize(Policy = "Required_Administrator_Role")]
public class AdminAccountController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<AppUser> _userManager;
    private readonly IMapper _mapper;

    public AdminAccountController(IUnitOfWork unitOfWork, UserManager<AppUser> userManager,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _mapper = mapper;
    }

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

            dto.Roles.Add("VIP");
            var roleResult = await _userManager.AddToRolesAsync(user, dto.Roles);

            if (!roleResult.Succeeded) return BadRequest(new ApiResponse(400, "Failed to add roles"));

            var admin = _mapper.Map<AdminDto>(user);
            admin.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            return Ok(new ApiOkResponse(admin));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

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
            return BadRequest(new ApiResponse(400, "You can't delete the user with admin role"));

        var orders = await _unitOfWork.OrdersRepository
            .GetOrdersForSpecificProduct(user.Id);

        foreach (var t in orders.Where(t => t != null))
        {
            _unitOfWork.OrdersRepository.DeleteOrder(t);
        }

        _unitOfWork.UserRepository.DeleteUser(user);

        if (!await _unitOfWork.Complete())
            return BadRequest(new ApiResponse(400, "Can't delete the user... something went wrong"));
        
        await _userManager.DeleteAsync(user);
        return Ok(new ApiResponse(200, "User deleted"));
    }
}