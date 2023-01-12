using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface IBenefitPercentInSpecificVipLevelRepository
{
    public Task<double?> GetBenefitPercentForProductAsync(int productId, int vipLevel);
    public void AddBenefitPercentForProduct(BenefitPercentInSpecificVilLevel tmp);
}