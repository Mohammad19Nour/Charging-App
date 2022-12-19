using AutoMapper;
using AutoMapper.QueryableExtensions;
using ChargingApp.DTOs;
using ChargingApp.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChargingApp.Data;

public class RechargeMethodRepository : IRechargeMethodeRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public RechargeMethodRepository(DataContext context , IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<RechargeMethodDto>?> GetRechargeMethodsAsync()
    {
        return await _context.RechargeMethods
            .Include(x => x.ChangerAndCompanies)
            .ProjectTo<RechargeMethodDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }
}