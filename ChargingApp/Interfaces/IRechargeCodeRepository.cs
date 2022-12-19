using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface IRechargeCodeRepository
{
    public Task<List<string>?> GenerateCodesWithValue(int numberOfCodes, int valueOfCode) ;
    public Task <RechargeCode?> GetCodeAsync(string code);

}