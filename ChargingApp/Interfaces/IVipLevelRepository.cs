using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface IVipLevelRepository
{
    public Task<List<VIPLevels>> GetAllVipLevelsAsync();
    public Task<int> GetVipLevelDiscount(int vipLevel);
    public Task<bool> CheckIfExist(int vipLevel);
    public Task<int> GetVipLevelForPurchasingAsync(double purchase);
} 