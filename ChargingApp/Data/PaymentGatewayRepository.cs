using ChargingApp.Entity;
using ChargingApp.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChargingApp.Data;

public class PaymentGatewayRepository : IPaymentGatewayRepository
{
    private readonly DataContext _context;

    public PaymentGatewayRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<PaymentGateway?> GetPaymentGatewayByNameAsync(string name)
    {
        name = name.ToLower();
        return await 
            _context.PaymentGateways.FirstOrDefaultAsync(x=>x.Name.ToLower() == name);
    }

    public async Task<bool> SaveAllChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}