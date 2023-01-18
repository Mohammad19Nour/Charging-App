using AutoMapper;
using AutoMapper.QueryableExtensions;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChargingApp.Data;

public class DebitRepository : IDebitRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public DebitRepository(DataContext context,IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public void AddDebit(DebitHistory debit)
    {
        _context.Debits.Add(debit);
    }

    public async Task<List<DebitDto>> GetDebits(DateQueryDto dto , string? userEmail = null)
    {
        userEmail = userEmail?.ToLower();
        
        var query = _context.Debits.Where(x => x.Date.Year == dto.Year);
        
        if (dto.Day != null)
            query = query.Where(x => x.Date.Day == dto.Day);
        if (dto.Month != null)
            query = query.Where(x => x.Date.Month == dto.Month);
        if (userEmail != null)
            query = query.Include(x => x.User)
                .Where(x => x.User.Email == userEmail);


        return await query.ProjectTo<DebitDto>(_mapper.ConfigurationProvider).ToListAsync();
    }
}