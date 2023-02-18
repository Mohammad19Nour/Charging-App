using ChargingApp.DTOs;
using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface ICurrencyRepository
{
    public Task<double> GetTurkishCurrency();
    public Task<double> GetSyrianCurrency();
    public Task<List<CurrencyDto>> GetCurrencies();
    public Task<bool> CheckIfExistByNameAsync(string name);
    public void UpdateByName(Currency currency);
    public Task<Currency?> GetCurrencyByNameAsync(string name);
}