using ChargingApp.Entity;
using ChargingApp.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace ChargingApp.Controllers;

[Authorize(Policy = "Required_Administrator_Role")]
public class AdminRolesController : AdminController
{
    private readonly UserManager<AppUser> _userManager;

    public AdminRolesController(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("users-with-roles")]
    public async Task<ActionResult> GetUserWithRoles()
    {
        var users = await _userManager.Users
            .Include(r => r.UserRoles)
            .ThenInclude(r => r.Role)
            .Select(u => new
            {
                u.Id,
                Roles = u.UserRoles
                    .Select(r => r.Role.Name.ToLower()
                    ).ToList(),
                UserEmail = u.Email,
                UserName = u.FirstName + " " + u.LastName
            })
            .ToListAsync();

        users = users.Where(x =>
            x.Roles.Any(s => s != "vip" && s != "normal")).ToList();

        var res = new[]
        {
            new
            {
                Id = 0, Roles = new List<string>(), UserEmail = string.Empty, UserName = string.Empty
            }
        }.ToList();

        res = users.Select(x => new
        {
            x.Id,
            Roles = x.Roles.Where(s => s != "vip" && s != "normal").ToList(),
            x.UserEmail,
            x.UserName
        }).ToList();
        return Ok(new ApiOkResponse(res));
    }

    [HttpPost("edit-roles/{userEmail}")]
    public async Task<ActionResult> EditRoles(string userEmail, [FromQuery] string roles)
    {
        userEmail = userEmail.ToLower();

        var selectedRoles = roles.Split(",").ToArray();
        var user = await _userManager.FindByEmailAsync(userEmail);

        if (user == null) return NotFound("user not found");

        if (selectedRoles.Length == 0)
            return BadRequest(new ApiResponse(400, "should specify some roles"));
        selectedRoles = selectedRoles.Select(x => x.ToLower()).ToArray();

        selectedRoles = selectedRoles.Where(x => x != "normal" && x != "vip").ToArray();


        if (selectedRoles.Length == 0)
            return BadRequest(new ApiResponse(400, "should specify some roles other than vip"));

        var userRoles = await _userManager.GetRolesAsync(user);
        userRoles = userRoles.Select(x => x.ToLower()).ToArray();
        selectedRoles = selectedRoles.Select(x => x.ToLower()).ToArray();

        var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

        if (!result.Succeeded)
            return BadRequest("Failed ll");
        result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

        if (!result.Succeeded)
            return BadRequest("Failed");

        var response = await _userManager.GetRolesAsync(user);

        response = response.Where(x => x.ToLower() != "normal" && x.ToLower() != "vip").ToList();
        return Ok(new ApiOkResponse(response));
    }
}