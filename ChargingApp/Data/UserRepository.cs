﻿using AutoMapper;
using ChargingApp.Entity;
using ChargingApp.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChargingApp.Data;

public class UserRepository : IUserRepository
{
    private readonly DataContext _context;

    public UserRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<AppUser?> GetUserByEmailAsync(string? email)
    {
        if (email is null) return null;
        email = email.ToLower();
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
        return user;
    }

    public void UpdateUserInfo(AppUser user)
    {
        _context.Entry(user).State = EntityState.Modified;
    }

    public void DeleteUser(AppUser user)
    {
        _context.Users.Remove(user);
    }

    public async Task<decimal> GetBenefitPercentAsync(string? email)
    {
        var user = await GetUserByEmailAsync(email);
        var vipLevel = user.VIPLevel;
        var x = await _context.VipLevels.Where(x => x.VipLevel == vipLevel).FirstAsync();
        decimal discount = x.BenefitPercent;
        return discount;
    }
}