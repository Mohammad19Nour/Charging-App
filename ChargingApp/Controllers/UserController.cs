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
    private readonly IUserRepository _userRepo;
    private readonly IMapper _mapper;
    private readonly UserManager<AppUser> _userManager;

    public UserController(IUserRepository userRepo, IMapper mapper,
        UserManager<AppUser> userManager)
    {
        _userRepo = userRepo;
        _mapper = mapper;
        _userManager = userManager;
    }

    [HttpPut("update-user-info")]
    public async Task<ActionResult> UpdateUser(UpdateUserInfoDto updateUserInfoDto)
    {
        var email = User.GetEmail();
        var user = await _userRepo.GetUserByEmailAsync(email);

        _mapper.Map(updateUserInfoDto, user);
        _userRepo.UpdateUserInfo(user);

        if (await _userRepo.SaveAllAsync()) return Ok(new ApiResponse(200, "Updated"));
        return BadRequest(new ApiResponse(400, "Failed to update"));
    }

    [HttpGet("user-info")]
    public async Task<ActionResult<UserInfoDto>> GetUserInfo()
    {
        var user = await _userRepo.GetUserByEmailAsync(User.GetEmail());

        return Ok(new ApiOkResponse(_mapper.Map<UserInfoDto>(user)));
    }

    [HttpPut("change-password")]
    public async Task<ActionResult> UpdatePassword([FromBody] UpdatePasswordDto dto)
    {
        var user = await _userRepo.GetUserByEmailAsync(User.GetEmail());
        if (user is null)
            return Unauthorized(new ApiResponse(403));
        var res = 
            await _userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);

        
        if (!res.Succeeded)
            return BadRequest(new ApiResponse(400, "Failed to update password"));

        return Ok(new ApiResponse(200, "updated successfully"));
    }

    [HttpGet("balance")]
    public async Task<ActionResult<double>> MyBalance()
    {
        var email = User.GetEmail();

        if (email is null) return Unauthorized(new ApiResponse(401));

        return Ok(new ApiOkResponse( (await _userRepo.GetUserByEmailAsync(email)).Balance));
    }
}