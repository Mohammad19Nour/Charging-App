using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface INotificationRepository
{
    public Task<List<OrderAndPaymentNotification>>
        GetNotificationForUserByEmailAsync(string userEmail);

    public void AddNotification(OrderAndPaymentNotification not);
    public void DeleteNotification(OrderAndPaymentNotification not);
    public void AddNotificationForHistoryAsync(NotificationHistory history);
    public Task<List<NotificationHistory>> GetNotificationHistoryByEmailAsync(string email);
}