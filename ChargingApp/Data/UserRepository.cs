﻿using AutoMapper;
using ChargingApp.Entity;
using ChargingApp.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChargingApp.Data;

public class UserRepository : IUserRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public UserRepository(DataContext context , IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
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

    public async Task<bool> SaveAllAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteUserByEmail(string? email)
    {
        var user = await GetUserByEmailAsync(email);
        if (user == null) return false;
        
        _context.Users.Remove(user);

        return await SaveAllAsync();
    }

    public async Task<int> GetDisCountAsync(string? email)
    {
        var user = await GetUserByEmailAsync(email);
        var vipLevel = user.VIPLevel;
        var x = await _context.VipLevels.Where(x => x.VIP_Level == vipLevel).FirstAsync();
        int discount = x.Discount;
        return discount;
    }
}