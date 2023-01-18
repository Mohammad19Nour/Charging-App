using ChargingApp.Entity;
using ChargingApp.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChargingApp.Data;

public class VipLevelRepository :IVipLevelRepository
{
    private readonly DataContext _context;

    public VipLevelRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<List<VIPLevel>> GetAllVipLevelsAsync()
    {
        return _context.VipLevels.ToList();
    }

    public async Task<double> GetBenefitPercentForVipLevel(int vipLevel)
    {
        var res = await _context.VipLevels.FirstAsync(x => x.VipLevel == vipLevel);
        
        return res.BenefitPercent;
    } 

    public async Task<bool> CheckIfExist(int vipLevel)
    {
        return await _context.VipLevels.FirstOrDefaultAsync(x => x.VipLevel == vipLevel) != null;
    }

    public async Task<int> GetVipLevelForPurchasingAsync(double purchase)
    {
        var v = await _context.VipLevels
            .Where(x=>x.VipLevel != 0)
            .OrderByDescending(x =>x.MinimumPurchase)
            .ThenByDescending(x=>x.VipLevel)
            .FirstOrDefaultAsync(x => x.MinimumPurchase <= purchase);
        Console.WriteLine(purchase);
        Console.WriteLine("\n\n**\n");
        return v.VipLevel;
    }

    public async Task<double> GetMinimumPurchasingForVipLevelAsync(int vipLevel)
    {
        var v = await _context.VipLevels
            .Where(x => x.VipLevel == vipLevel)
            .FirstAsync();
        return v.MinimumPurchase;
    }

    public async Task<VIPLevel?> GetVipLevelAsync(int vipLevel)
    {
        return await _context.VipLevels.FirstOrDefaultAsync(x => x.VipLevel == vipLevel);
    }
}