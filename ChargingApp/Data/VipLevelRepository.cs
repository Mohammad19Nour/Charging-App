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

    public async Task<int> GetVipLevelDiscount(int vipLevel)
    {
        var ress = await _context.VipLevels.FirstOrDefaultAsync(x => x.VIP_Level == vipLevel);

        return ress.Discount;
    }

    public async Task<bool> CheckIfExist(int vipLevel)
    {
        return await _context.VipLevels.FirstOrDefaultAsync(x => x.VIP_Level == vipLevel) != null;
    }

    public async Task<int> GetVipLevelForPurchasingAsync(double purchase)
    {
        var v = await _context.VipLevels
            .OrderByDescending(x=>x.MinimumPurchase)
            .FirstOrDefaultAsync(x => x.MinimumPurchase < purchase);
        return v.VIP_Level;
    }
}