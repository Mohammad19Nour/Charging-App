using ChargingApp.Entity;
using ChargingApp.Interfaces;
using ChargingApp.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace ChargingApp.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<PresenceHub> _presenceHub;
    private readonly PresenceTracker _tracker;

    public NotificationService(IHubContext<PresenceHub> presenceHub, PresenceTracker tracker)
    {
        _presenceHub = presenceHub;
        _tracker = tracker;
    }

    public async Task<bool> NotifyUserByEmail(string userEmail, IUnitOfWork unitOfWork,
        OrderAndPaymentNotification notification, string methodName, object returnedArgs)
    {
        var connections = await _tracker.GetConnectionsForUser(userEmail);

        if (connections != null)
        {
            await _presenceHub.Clients.Clients(connections)
                .SendAsync(methodName, returnedArgs);
            return true;
        }

        unitOfWork.NotificationRepository.AddNotification(notification);
        return false;
    }

    public async Task<bool> VipLevelNotification(string userEmail, string methodName, object returnedArgs)
    {
        var connections = await _tracker.GetConnectionsForUser(userEmail);

        if (connections == null) return true;
        await _presenceHub.Clients.Clients(connections)
            .SendAsync(methodName, returnedArgs);
        return true;

    }
}