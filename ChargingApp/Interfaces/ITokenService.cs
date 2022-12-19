using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface ITokenService
{
    Task<string> CreateToken(AppUser user);
}