using System.Security.Claims;

namespace Charging_App.Extentions;

public static class ClaimsPrincipleExtentions
{
    public static string GetEmail(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Email)?.Value;
    }
}