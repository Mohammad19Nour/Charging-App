using ChargingApp.DTOs;
using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface IPaymentRepository
{
    public Task<List<CompanyPaymentDto>?> GetPaymentsForUserAsync(string userEmail);
    public void AddPayment(Payment payment);
    public Task<ChangerAndCompany?> GetPaymentAgentByIdAsync(int? id);
    public Task<Payment?> GetPaymentByIdAsync(int id);
    public Task<List<PaymentAdminDto>> GetAllPendingPaymentsAsync();
} 