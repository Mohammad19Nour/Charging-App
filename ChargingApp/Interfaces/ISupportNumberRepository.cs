using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface ISupportNumberRepository
{
    public void AddSupportNumber(SupportNumber support);
    public Task<List<string>> GetSupportNumbersAsync();
    public Task<SupportNumber?> GetSupportNumberByIdAsync(int id);
}