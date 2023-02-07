using ChargingApp.Entity;
using ChargingApp.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChargingApp.Data;

public class SupportNumberRepository : ISupportNumberRepository
{
    private readonly DataContext _context;

    public SupportNumberRepository(DataContext context)
    {
        _context = context;
    }

    public void AddSupportNumber(SupportNumber support)
    {
        _context.SupportNumbers.Add(support);
    }

    public async Task<List<string>> GetSupportNumbersAsync()
    {
        var res = await _context.SupportNumbers.ToListAsync();
        return res.Select(x => x.PhoneNumber).ToList();
    }

    public async Task<SupportNumber?> GetSupportNumberByIdAsync(int id)
    {
        return await _context.SupportNumbers.Where(x => x.Id == id)
            .FirstOrDefaultAsync();
    }
}