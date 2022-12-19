using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface IPaymentGatewayRepository
{
    public Task<PaymentGateway?> GetPaymentGatewayByIdAsync(int id);
    public Task<bool> SaveAllChangesAsync();
}