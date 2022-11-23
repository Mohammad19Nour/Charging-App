using Charging_App.Entity;

namespace Charging_App.Interfaces;

public interface ITokenService
{
    Task<string> CreateToken(AppUser user);
}