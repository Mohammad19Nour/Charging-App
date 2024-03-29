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
            .Include(x => x.Payment!.Photo)
            .Include(x => x.Order!.Photo)
            .Where(x => x.User.Email.ToLower() == userEmail)
            .OrderByDescending(x => x.Order!.CreatedAt)
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

    public void DeleteNotificationHistory(NotificationHistory[] nots)
    {
        if (nots.Length > 0)
            _context.NotificationsHistory.RemoveRange(nots);
    }

    public void AddNotificationForHistoryAsync(NotificationHistory history)
    {
        _context.NotificationsHistory.Add(history);
    }

    public async Task<List<NotificationHistory>> GetNotificationHistoryByEmailAsync(string email)
    {
        email = email.ToLower();
        return await _context.NotificationsHistory
            .Include(x => x.User)
            .Where(x => x.User.Email.ToLower() == email)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<OrderAndPaymentNotification?>> GetOrdersNotifications(int orderId)
    {
        return await _context.OrderAndPaymentNotifications
            .Include(o => o.Order)
            .Where(x => x.Order.Id == orderId)
            .Where(o => o.Order != null)
            .ToListAsync();
    }

    public async Task<List<OrderAndPaymentNotification?>> GetPaymentsNotifications(int paymentId)
    {
        return await _context.OrderAndPaymentNotifications
            .Include(o => o.Payment)
            .Where(x => x.Payment.Id == paymentId)
            .Where(o => o.Payment != null)
            .ToListAsync();
    }

    public IQueryable<NotificationHistory> GetQueryable()
    {
        return _context.NotificationsHistory.AsQueryable();
    }
}