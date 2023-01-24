using ChargingApp.DTOs;
using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface INotificationRepository
{
    public Task<List<OrderAndPaymentNotification>>
        GetNotificationForUserByEmailAsync(string userEmail);

    public void AddNotification(OrderAndPaymentNotification not);
    public void DeleteNotification(OrderAndPaymentNotification not);
}