using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface IUserRepository
{
    public Task<AppUser?> GetUserByEmailAsync(string? email);
    public void UpdateUserInfo(AppUser user) ;
    public Task<bool> DeleteUserByEmail(string? email);
    public Task<int> GetBenefitPercentAsync(string? email);
}