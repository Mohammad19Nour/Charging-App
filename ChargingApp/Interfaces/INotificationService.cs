using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface INotificationService
{
    public Task<bool> NotifyUserByEmail(string userEmail , IUnitOfWork unitOfWork 
        , OrderAndPaymentNotification notification, string methodName , object returnedArgs );
    
    public Task<bool> VipLevelNotification(string userEmail ,string methodName , object returnedArgs );
}