using AutoMapper;
using AutoMapper.QueryableExtensions;
using ChargingApp.DTOs;
using ChargingApp.Entity;
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
           // .Include(x => x.ChangerAndCompanies)
            .ProjectTo<RechargeMethodDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public void AddAgent(RechargeMethod method, ChangerAndCompany agent)
    {
        method.ChangerAndCompanies ??= new List<ChangerAndCompany>();

        method.ChangerAndCompanies.Add(agent);
    }

    public async Task<bool> DeleteAgent(int agentId)
    {
        var agent = await _context.ChangerAndCompanies
            .FirstOrDefaultAsync(x => x.Id == agentId);
        if (agent is null) return false;
        
        _context.ChangerAndCompanies.Remove(agent);
        return await SaveAllChangesAsync();
    }

    public async Task<RechargeMethod?> GetRechargeMethodByIdAsync(int methodId)
    {
        return await _context.RechargeMethods
            .Include(x=>x.ChangerAndCompanies)
            .FirstOrDefaultAsync(x => x.Id == methodId);
        
    }

    public async Task<bool> SaveAllChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

}