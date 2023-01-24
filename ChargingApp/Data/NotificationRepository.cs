﻿using ChargingApp.Entity;
using ChargingApp.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChargingApp.Data;

public class NotificationRepository : INotificationRepository
{
    private readonly DataContext _context;

    public NotificationRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<List<OrderAndPaymentNotification>> GetNotificationForUserByEmailAsync(string userEmail)
    {
        userEmail = userEmail.ToLower();
        return await _context.OrderAndPaymentNotifications
            .Include(x => x.User)
            .Include(x => x.Order)
            .Include(x => x.Payment)
            .Include(x=>x.Payment!.Photo)
            .Where(x => x.User.Email.ToLower() == userEmail)
            .ToListAsync();
    }

    public void AddNotification(OrderAndPaymentNotification not)
    {
        _context.OrderAndPaymentNotifications.Add(not);
    }

    public void DeleteNotification(OrderAndPaymentNotification not)
    {
        _context.OrderAndPaymentNotifications.Remove(not);
    }
}