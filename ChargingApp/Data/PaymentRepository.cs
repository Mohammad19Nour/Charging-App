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
            .Include(p=>p.Photo)
            .OrderByDescending(x=>x.CreatedDate)
            .ProjectTo<CompanyPaymentDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public void AddPayment(Payment payment)
    {
        _context.Payments.Add(payment);
    }

    public async Task<ChangerAndCompany?> GetPaymentAgentByIdAsync(int? id)
    {
        if (id is null) return null;
        
        return await _context.ChangerAndCompanies
            .FirstOrDefaultAsync(x=>x.Id == id);
    }

    public async Task<Payment?> GetPaymentByIdAsync(int id)
    {
        return await _context.Payments
            .Where(p => p.Id == id)
            .Include(u=>u.User)
            .Include(p=>p.Photo)
            .FirstOrDefaultAsync();
    }

    public async Task<List<PaymentAdminDto>> GetAllPendingPaymentsAsync()
    {
        return await _context.Payments
            .Where(c=>!c.Checked)
            .Include(u=>u.User)
            .Include(p=>p.Photo)
            .OrderByDescending(p=>p.CreatedDate)
            .ProjectTo<PaymentAdminDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }
}