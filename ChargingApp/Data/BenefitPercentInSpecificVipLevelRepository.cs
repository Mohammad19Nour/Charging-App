using ChargingApp.Entity;
using ChargingApp.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChargingApp.Data;

public class BenefitPercentInSpecificVipLevelRepository : IBenefitPercentInSpecificVipLevelRepository
{
    private readonly DataContext _context;

    public BenefitPercentInSpecificVipLevelRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<double?> GetBenefitPercentForProductAsync(int productId, int vipLevel)
    {
        var tmp = await _context.SpecificBenefit
            .Where(x => x.ProductId == productId)
            .Where(x=> x.VipLevel == vipLevel)
            .FirstOrDefaultAsync();

        return tmp?.BenefitPercent;
    }

    public void AddBenefitPercentForProduct(BenefitPercentInSpecificVilLevel tmp)
    {
        _context.SpecificBenefit.Add(tmp);
    }
}