using AutoMapper;
using AutoMapper.QueryableExtensions;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChargingApp.Data;

public class PaymentRepository : IPaymentRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public PaymentRepository(DataContext context , IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    } 

    public async Task<List<CompanyPaymentDto>?> GetPaymentsForUserAsync(string userEmail)
    {
        userEmail = userEmail.ToLower();
       return await _context.Payments
            .Where(x => x.User.Email == userEmail)
            .ProjectTo<CompanyPaymentDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public void AddPayment(Payment payment)
    {
        _context.Payments.Add(payment);
    }

    public async Task<bool> SaveAllChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<ChangerAndCompany?> GetPaymentAgentByIdAsync(int? id)
    {
        if (id is null) return null;
        
        return await _context.ChangerAndCompanies
            .FirstOrDefaultAsync(x=>x.Id == id);
    }
}