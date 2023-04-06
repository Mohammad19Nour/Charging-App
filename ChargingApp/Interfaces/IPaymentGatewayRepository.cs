using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface IPaymentGatewayRepository
{
    public Task<PaymentGateway?> GetPaymentGatewayByNameAsync(string name);
    public Task<List<PaymentGateway>> GetPaymentGatewaysAsync();
    public Task<PaymentGateway?> GetPaymentGatewayByIdAsync(int id);
    public void UpdateGateway(PaymentGateway gateway);
}