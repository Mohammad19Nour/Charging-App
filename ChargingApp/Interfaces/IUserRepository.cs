using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface IUserRepository
{
    public Task<AppUser?> GetUserByEmailAsync(string? email);
    public void UpdateUserInfo(AppUser user) ;
    public Task<bool> SaveAllAsync();
    public Task<bool> DeleteUserByEmail(string? email);
    public Task<int> GetDisCountAsync(string? email);
}