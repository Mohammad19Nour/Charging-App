using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface IUserRepository
{
    public Task<AppUser?> GetUserByEmailAsync(string? email);
    public void UpdateUserInfo(AppUser user) ;
    public void DeleteUser(AppUser user);
    public Task<decimal> GetBenefitPercentAsync(string? email);
}