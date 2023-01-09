using ChargingApp.DTOs;
using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface IOurAgentsRepository
{
    public Task<List<OurAgentsDto>> GetOurAgentsAsync();
    public void AddAgent(OurAgent newAgent);
}