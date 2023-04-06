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
            _context.PaymentGateways.FirstOrDefaultAsync(x=>x.EnglishName.ToLower() == name);
    }

    public async Task<List<PaymentGateway>> GetPaymentGatewaysAsync()
    {
        return await _context.PaymentGateways.
            
            ToListAsync();
    }

    public async Task<PaymentGateway?> GetPaymentGatewayByIdAsync(int id)
    {
        return await _context.PaymentGateways
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public void UpdateGateway(PaymentGateway gateway)
    {
        _context.PaymentGateways.Update(gateway);
    }
}