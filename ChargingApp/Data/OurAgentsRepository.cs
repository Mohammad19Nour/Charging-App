using AutoMapper;
using AutoMapper.QueryableExtensions;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChargingApp.Data;

public class OurAgentsRepository : IOurAgentsRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public OurAgentsRepository(DataContext context , IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<OurAgentsDto>> GetOurAgentsAsync()
    {
        return await _context.OurAgents.ProjectTo<OurAgentsDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

    public void AddAgent(OurAgent newAgent)
    {
        _context.OurAgents.Add(newAgent);
    }
}