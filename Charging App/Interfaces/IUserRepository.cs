using Charging_App.Entity;

namespace Charging_App.Interfaces;

public interface IUserRepository
{
    public Task<AppUser> GetUserByEmailAsync(string email);
    public void UpdateUserInfo(AppUser user) ;
    public Task<bool> SaveAllAsync();
}