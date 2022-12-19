using ChargingApp.DTOs;

namespace ChargingApp.Interfaces;

public interface IRechargeMethodeRepository
{
    public Task<List<RechargeMethodDto>?> GetRechargeMethodsAsync();
}