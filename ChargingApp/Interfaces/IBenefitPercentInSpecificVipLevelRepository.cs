using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface IBenefitPercentInSpecificVipLevelRepository
{
    public Task<double?> GetBenefitPercentForProductAsync(int productId, int vipLevel);
    public void AddBenefitPercentForProduct(BenefitPercentInSpecificVilLevel tmp);
    public Task<bool> CheckIfExist(int productId , int vipLevel);
    public Task<BenefitPercentInSpecificVilLevel?> GetBenefitAsync(int productId , int vipLevel);
}