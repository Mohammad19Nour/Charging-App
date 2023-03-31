using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface ISpecificPriceForUserRepository
{
    public Task<decimal?> GetProductPriceForUserAsync(int productId, AppUser user);
    public void AddProductPriceForUser(SpecificPriceForUser newRow);
    public void UpdateProductPriceForUser(SpecificPriceForUser newRow);
    public Task<SpecificPriceForUser?> GetPriceForUserAsync(string email , int vipLevel, int productId);

}