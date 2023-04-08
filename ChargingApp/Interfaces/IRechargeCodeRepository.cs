using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface IRechargeCodeRepository
{
    public Task<List<string>?> GenerateCodesWithValue(int numberOfCodes, int valueOfCode) ;
    public Task <RechargeCode?> GetCodeAsync(string code);
    public Task<List<RechargeCode>> GetCodesForUserAsync(int userId);
    public void Update(RechargeCode code);
}