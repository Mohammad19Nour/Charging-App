using System.Security.Claims;
using ChargingApp.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChargingApp.Extentions;

public static class ClaimsPrincipleExtentions
{
    public static string? GetEmail(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Email)?.Value.ToLower();
    }

  /*  public static async Task<IEnumerable<string>> GetRoles(this ClaimsPrincipal user , UserManager<AppUser> userManager)
    {
        var userEmail = user.GetEmail().ToLower();
        
        return await userManager.Users
            .Where(x => x.Email == userEmail)
            .Include(x=>x.UserRoles)
            .ThenInclude(x => x.Role)
            .Select(u => u.UserRoles.Select(x => x.Role.Name))
            .FirstAsync();
    }*/
    

    public static IEnumerable<string> GetRoles(this ClaimsPrincipal principal)
    {
        return principal.Identities.SelectMany(i =>
        {
            return i.Claims
                .Where(c => c.Type == i.RoleClaimType)
                .Select(c => c.Value)
                .ToList();
        });
    }
}