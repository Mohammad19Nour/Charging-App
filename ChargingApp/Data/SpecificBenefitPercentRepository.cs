using ChargingApp.Entity;
using ChargingApp.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChargingApp.Data;

public class SpecificBenefitPercentRepository : ISpecificBenefitPercentRepository
{
    private readonly DataContext _context;

    public SpecificBenefitPercentRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<decimal?> GetBenefitPercentForProductAsync(int productId)
    {
        var tmp = await _context.SpecificBenefit
            .Where(x => x.ProductId == productId)
            .FirstOrDefaultAsync();

        return tmp?.BenefitPercent;
    }

    public void AddBenefitPercentForProduct(SpecificBenefitPercent tmp)
    {
        _context.SpecificBenefit.Add(tmp);
    }


    public async Task<SpecificBenefitPercent?> GetBenefitAsync(int productId)
    {
        return await _context.SpecificBenefit
            .FirstOrDefaultAsync(x => x.ProductId == productId);
    }

    public async Task<List<SpecificBenefitPercent>> GetAllSpecificBenefitPercentAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<bool> CheckIfExist(int productId, int vipLevel)
    {
        return await _context.SpecificBenefit.AsNoTracking()
            .FirstOrDefaultAsync
                (x => x.ProductId == productId) != null;
    }

    public async Task<SpecificBenefitPercent?> GetBenefitAsync(int productId, int vipLevel)
    {
        return await _context.SpecificBenefit
            .FirstOrDefaultAsync(x => x.ProductId == productId);
    }
}