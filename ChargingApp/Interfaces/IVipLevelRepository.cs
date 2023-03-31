using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface IVipLevelRepository
{
    public Task<List<VIPLevel>> GetAllVipLevelsAsync();
    public Task<decimal> GetBenefitPercentForVipLevel(int vipLevel);
    public Task<bool> CheckIfExist(int vipLevel);
    public Task<int> GetVipLevelForPurchasingAsync(decimal purchase);
    public Task<decimal> GetMinimumPurchasingForVipLevelAsync(int vipLevel);
    public Task<VIPLevel?> GetVipLevelAsync(int vipLevel);
    public Task<bool> CheckIfMinimumPurchasingIsValidAsync(decimal minPurchasing);
    public void UpdateVipLevel(VIPLevel vip);
    public void AddVipLevel(VIPLevel vip);
} 