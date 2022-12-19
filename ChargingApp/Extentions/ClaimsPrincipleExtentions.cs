using System.Security.Claims;

namespace ChargingApp.Extentions;

public static class ClaimsPrincipleExtentions
{
    public static string? GetEmail(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Email)?.Value.ToLower();
    }
    public static bool HasAtribute(this object objectToCheck, string methodName)
    {
        var type = objectToCheck.GetType();
        Console.WriteLine(type.GetProperty(methodName).Name+"frfrfrf\n\n\n");
        return type.GetMethod(methodName) != null;
    } 
}