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
            .Include(x=>x.Photo)
            .Include(x => x.ChangerAndCompanies)
            .ThenInclude(x=>x.Photo)
            .ProjectTo<RechargeMethodDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public void AddAgent(RechargeMethod method, ChangerAndCompany agent)
    {
        method.ChangerAndCompanies.Add(agent);
    }

    public  void DeleteAgent(ChangerAndCompany agent)
    {
        _context.ChangerAndCompanies.Remove(agent);
    }

    public async Task<RechargeMethod?> GetRechargeMethodByIdAsync(int methodId)
    {
        return await _context.RechargeMethods
            .Include(x=>x.Photo)
            .Include(x => x.ChangerAndCompanies)
            .ThenInclude(x=>x.Photo)
            .FirstOrDefaultAsync(x => x.Id == methodId);
    }

    public void Update(RechargeMethod method)
    {
        _context.RechargeMethods.Update(method);
    }

    public void UpdateAgent(ChangerAndCompany agent)
    {
        _context.ChangerAndCompanies.Update(agent);
    }
}