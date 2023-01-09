using AutoMapper;
using AutoMapper.QueryableExtensions;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChargingApp.Data;

public class CurrencyRepository : ICurrencyRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public CurrencyRepository(DataContext context , IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    private async Task<double> GetCurrencyByName(string currencyName)
    {
        currencyName = currencyName.ToLower();
        return (await _context.Currencies
            .Where(x => x.Name.ToLower() == currencyName )
            .FirstAsync()
            ).ValuePerDollar;

    }

    public async Task<double> GetTurkishCurrency()
    {
        return await GetCurrencyByName("turkish");
    }

    public async Task<double> GetSyrianCurrency()
    {
        return await GetCurrencyByName("syrian");
    }

    public async Task<bool> SetCurrencyValeByName(string currencyName, double value)
    {
        
        currencyName = currencyName.ToLower();
        var currency = await _context.Currencies
            .FirstOrDefaultAsync(x => x.Name.ToLower() == currencyName);
        
        if (currency is null) return false;

        currency.ValuePerDollar = value;
        return true;
    }

    public async Task<List<CurrencyDto>> GetCurrencies()
    {
        return await _context.Currencies
            .ProjectTo<CurrencyDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<bool> CheckIfExistByNameAsync(string name)
    {
        var res = await _context.Currencies.FirstOrDefaultAsync(x => x.Name.ToLower() == name);
        return res != null;
    }

    public async void UpdateByNameAsync(string name , double value)
    {
        var res = await _context.Currencies.FirstOrDefaultAsync(x => x.Name.ToLower() == name);
        if (res != null) res.ValuePerDollar = value;
    }
}