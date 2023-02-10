using ChargingApp.DTOs;
using ChargingApp.Entity;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Interfaces;

public interface IRechargeMethodeRepository
{
    public Task<List<RechargeMethodDto>?> GetRechargeMethodsAsync();
    public void AddAgent(RechargeMethod method , ChangerAndCompany agent);
    public void DeleteAgent(ChangerAndCompany agent);
    public Task<RechargeMethod?> GetRechargeMethodByIdAsync(int methodId);
    public void Update(RechargeMethod method);
    public void UpdateAgent(ChangerAndCompany agent);
}