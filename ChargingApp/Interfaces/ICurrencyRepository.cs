using ChargingApp.DTOs;
using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface ICurrencyRepository
{
    public Task<double> GetTurkishCurrency();
    public Task<double> GetSyrianCurrency();
    public Task<bool> SetCurrencyValeByName(string currencyName , double value);
    public Task<List<CurrencyDto>> GetCurrencies();
    public Task<bool> CheckIfExistByNameAsync(string name);
    public void UpdateByNameAsync(string name , double value);
}