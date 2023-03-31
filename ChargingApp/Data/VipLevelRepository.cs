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
        return await _context.VipLevels
            .Include(x=>x.Photo)
            .ToListAsync();
    }

    public async Task<decimal> GetBenefitPercentForVipLevel(int vipLevel)
    {
        var res = await _context.VipLevels
            .FirstAsync(x => x.VipLevel == vipLevel);
        
        return res.BenefitPercent;
    } 

    public async Task<bool> CheckIfExist(int vipLevel)
    {
        return await _context.VipLevels.FirstOrDefaultAsync(x => x.VipLevel == vipLevel) != null;
    }

    public async Task<int> GetVipLevelForPurchasingAsync(decimal purchase)
    {
        var res = await _context.VipLevels
            .Where(x => x.VipLevel != 0)
            .Where(x => x.MinimumPurchase <= purchase)
            .OrderByDescending(x => x.MinimumPurchase)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        return res?.VipLevel ?? 1;
    }

    public async Task<decimal> GetMinimumPurchasingForVipLevelAsync(int vipLevel)
    {
        var v = await _context.VipLevels
            .Where(x => x.VipLevel == vipLevel)
            .FirstAsync();
        return v.MinimumPurchase;
    }

    public async Task<VIPLevel?> GetVipLevelAsync(int vipLevel)
    {
        return await _context.VipLevels
            .Include(x=>x.Photo)
            .FirstOrDefaultAsync(x => x.VipLevel == vipLevel);
    }

    public async Task<bool> CheckIfMinimumPurchasingIsValidAsync(decimal minPurchasing)
    {
        return await _context.VipLevels
            .Where(x=>x.VipLevel > 0)
            .Where(x=>x.MinimumPurchase >= minPurchasing )
            .FirstOrDefaultAsync() == null;
        
    }

    public void UpdateVipLevel(VIPLevel vip)
    {
        _context.Entry(vip).State = EntityState.Modified;
    }

    public void AddVipLevel(VIPLevel vip)
    {
        _context.VipLevels.Add(vip);
    }
}