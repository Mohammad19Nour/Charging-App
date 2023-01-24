using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface IVipLevelRepository
{
    public Task<List<VIPLevel>> GetAllVipLevelsAsync();
    public Task<double> GetBenefitPercentForVipLevel(int vipLevel);
    public Task<bool> CheckIfExist(int vipLevel);
    public Task<int> GetVipLevelForPurchasingAsync(double purchase);
    public Task<double> GetMinimumPurchasingForVipLevelAsync(int vipLevel);
    public Task<VIPLevel?> GetVipLevelAsync(int vipLevel);
    public void UpdateVipLevel(VIPLevel vip);
} 