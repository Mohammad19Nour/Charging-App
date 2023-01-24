namespace ChargingApp.SignalR;

public interface IPresenceTracker
{
    public Task UserConnected(string username, string connectionId);
    public Task UserDisConnected(string username, string connectionId);
    public Task<string[]> GetOnlineUsers();
}