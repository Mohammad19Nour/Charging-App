using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface INotificationRepository
{
    public Task<List<OrderAndPaymentNotification>>
        GetNotificationForUserByEmailAsync(string userEmail);

    public void AddNotification(OrderAndPaymentNotification not);
    public void DeleteNotification(OrderAndPaymentNotification not);
    public void DeleteNotificationHistory(NotificationHistory[] nots);
    public void AddNotificationForHistoryAsync(NotificationHistory history);
    public Task<List<NotificationHistory>> GetNotificationHistoryByEmailAsync(string email);
    public Task<List<OrderAndPaymentNotification?>> GetOrdersNotifications(int orderI);
    public Task<List<OrderAndPaymentNotification?>> GetPaymentsNotifications(int paymentId);
    public IQueryable<NotificationHistory> GetQueryable();
}