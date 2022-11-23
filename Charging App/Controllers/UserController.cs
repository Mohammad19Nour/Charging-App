using AutoMapper;
using Charging_App.DTOs;
using Charging_App.Extentions;
using Charging_App.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Charging_App.Controllers;

[Authorize]
public class UserController : BaseApiController
{
    private readonly IUserRepository _repo;
    private readonly IMapper _mapper;

    public UserController(IUserRepository repo , IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(UpdateUserInfoDto updateUserInfoDto)
    {
        var email = User.GetEmail();
        var user = await _repo.GetUserByEmailAsync(email);

        _mapper.Map(updateUserInfoDto, user);
        _repo.UpdateUserInfo(user);

        if (await _repo.SaveAllAsync()) return NoContent();
        return BadRequest("Failed to update");
    }
}