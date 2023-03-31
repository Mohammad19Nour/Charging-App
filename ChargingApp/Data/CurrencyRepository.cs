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

    public CurrencyRepository(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    private async Task<decimal> GetCurrencyByName(string currencyName)
    {
        currencyName = currencyName.ToLower();
        return (await _context.Currencies
                .AsNoTracking()
                .Where(x => x.Name.ToLower() == currencyName)
                .FirstAsync()
            ).ValuePerDollar;
    }

    public async Task<decimal> GetTurkishCurrency()
    {
        return await GetCurrencyByName("turkish");
    }

    public async Task<decimal> GetSyrianCurrency()
    {
        return await GetCurrencyByName("syrian");
    }

    public async Task<List<CurrencyDto>> GetCurrencies()
    {
        return await _context.Currencies
            .ProjectTo<CurrencyDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<bool> CheckIfExistByNameAsync(string name)
    {
        var res = await _context.Currencies
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name.ToLower() == name);
        return res != null;
    }

    public void UpdateByName(Currency currency)
    {
        _context.Currencies.Update(currency);
    }

    public async Task<Currency?> GetCurrencyByNameAsync(string name)
    {
        return await _context.Currencies
            .FirstOrDefaultAsync(x => x.Name.ToLower() == name);
    }
}