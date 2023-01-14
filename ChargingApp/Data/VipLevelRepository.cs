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

    public async Task<List<VIPLevels>> GetAllVipLevelsAsync()
    {
        return _context.VipLevels.ToList();
    }

    public async Task<double> GetBenefitPercentForVipLevel(int vipLevel)
    {
        var res = await _context.VipLevels.FirstAsync(x => x.VIP_Level == vipLevel);
        
        return res.BenefitPercent;
    } 

    public async Task<bool> CheckIfExist(int vipLevel)
    {
        return await _context.VipLevels.FirstOrDefaultAsync(x => x.VIP_Level == vipLevel) != null;
    }

    public async Task<int> GetVipLevelForPurchasingAsync(double purchase)
    {
        var v = await _context.VipLevels
            .OrderByDescending(x =>x.MinimumPurchase)
            .ThenByDescending(x=>x.VIP_Level)
            .FirstOrDefaultAsync(x => x.MinimumPurchase <= purchase);
        return v.VIP_Level;
    }

    public async Task<double> GetMinimumPurchasingForVipLevelAsync(int vipLevel)
    {
        var v = await _context.VipLevels
            .Where(x => x.VIP_Level == vipLevel)
            .FirstAsync();
        return v.MinimumPurchase;
    }

    public async Task<VIPLevels?> GetVipLevelAsync(int vipLevel)
    {
        return await _context.VipLevels.FirstOrDefaultAsync(x => x.VIP_Level == vipLevel);
    }
}