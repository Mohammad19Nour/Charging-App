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