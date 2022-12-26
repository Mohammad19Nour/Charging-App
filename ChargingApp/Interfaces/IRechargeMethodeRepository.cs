using ChargingApp.DTOs;
using ChargingApp.Entity;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Interfaces;

public interface IRechargeMethodeRepository
{
    public Task<List<RechargeMethodDto>?> GetRechargeMethodsAsync();
    public void AddAgent(RechargeMethod method , ChangerAndCompany agent);
    public Task<bool> DeleteAgent(int agentId);
    public Task<RechargeMethod?> GetRechargeMethodByIdAsync(int methodId);

    public Task<bool> SaveAllChangesAsync();
}