﻿using ChargingApp.Entity;
using ChargingApp.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChargingApp.Data;

public class SpecificPriceForUserRepository : ISpecificPriceForUserRepository
{
    private readonly DataContext _context;

    public SpecificPriceForUserRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<decimal?> GetProductPriceForUserAsync(int productId, AppUser user)
    {
        //Console.WriteLine(productId + " " + user.Id + " "+ user.VIPLevel + "\n\n-**");
        return (await _context.SpecificPriceForUsers
            .AsNoTracking()
            .Where(x =>
                x.ProductId == productId && user.Id == x.User.Id && x.VipLevel == user.VIPLevel)
            .FirstOrDefaultAsync())?.ProductPrice;
    }

    public void AddProductPriceForUser(SpecificPriceForUser newRow)
    {
        _context.SpecificPriceForUsers.Add(newRow);
    }

    public void UpdateProductPriceForUser(SpecificPriceForUser newRow)
    {
        _context.Entry(newRow).State = EntityState.Modified;
    }

    public async Task<SpecificPriceForUser?> GetPriceForUserAsync(string email , int vipLevel, int productId)
    {
     //   Console.WriteLine(productId + " " + vipLevel + " " + email + "\n\n-**");

         var h = await _context.SpecificPriceForUsers
            .Include(x => x.User)
            .Where(x =>
                x.ProductId == productId && x.User.Email.ToLower() == email 
                                         && vipLevel == x.VipLevel)
          //  .AsNoTracking()
            .FirstOrDefaultAsync();

        // Console.WriteLine(h.User.Email);
         return h;
    }
}