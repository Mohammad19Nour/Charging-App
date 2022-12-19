using ChargingApp.DTOs;
using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface IPaymentRepository
{
    public Task<List<PaymentDto>?> GetPaymentsForUserAsync(string userEmail);
    public void AddPayment(Payment payment);
    public Task<bool> SaveAllChangesAsync();
    public Task<ChangerAndCompany?> GetPaymentAgentByIdAsync(int? id);
} 