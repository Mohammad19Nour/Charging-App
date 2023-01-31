using ChargingApp.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            .ThenInclude(r=>r.Role)
            .Select(u=> new
            {
                u.Id,
                Roles = u.UserRoles.Select(r => r.Role.Name).ToList(),
                UserEmail = u.Email,
                UserName = u.FirstName + " " + u.LastName
            })
            .ToListAsync();
        return Ok(users);
    }

    [HttpPost("edit-roles/{userEmail}")]
    public async Task<ActionResult> EditRoles(string userEmail, [FromQuery] string roles)
    {
        userEmail = userEmail.ToLower();
        
        var selectedRoles = roles.Split(",").ToArray();
        var user = await _userManager.FindByEmailAsync(userEmail);

        if (user == null) return NotFound("user not found");
        
        var userRoles = await _userManager.GetRolesAsync(user);
        var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

        if (!result.Succeeded)
            return BadRequest("Failed");
        result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
        
        if (!result.Succeeded)
            return BadRequest("Failed");

        return Ok(await _userManager.GetRolesAsync(user));

    }
    
}