using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface ISpecificBenefitPercentRepository
{
    public Task<decimal?> GetBenefitPercentForProductAsync(int productId);
    public void AddBenefitPercentForProduct(SpecificBenefitPercent tmp);
    public Task<SpecificBenefitPercent?> GetBenefitAsync(int productId );
    public Task<List<SpecificBenefitPercent>> GetAllSpecificBenefitPercentAsync();
}