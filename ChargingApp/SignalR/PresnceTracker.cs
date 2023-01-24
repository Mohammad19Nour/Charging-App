namespace ChargingApp.SignalR;

public class PresenceTracker
{
    private static readonly Dictionary<string, List<string>?> OnlineUsers = 
        new Dictionary<string, List<string>?>();

    public Task UserConnected(string userEmail, string connectionId)
    {
        lock (OnlineUsers)
        {
            if (OnlineUsers.ContainsKey(userEmail))
            {
                OnlineUsers[userEmail]?.Add(connectionId);
            }
            else OnlineUsers.Add(userEmail , new List<string>{connectionId});
        }
        return Task.CompletedTask;
    }
    
    public Task UserDisConnected(string userEmail, string connectionId)
    {
        lock (OnlineUsers)
        {
            if (!OnlineUsers.ContainsKey(userEmail)) return Task.CompletedTask;
            OnlineUsers[userEmail]?.Remove(connectionId);
            if (OnlineUsers[userEmail]!.Count == 0) OnlineUsers.Remove(userEmail);
        }
        return Task.CompletedTask;
    }

    public Task<List<string>?> GetConnectionsForUser(string userEmail)
    {
        List<string>? connectionIds;
        lock (OnlineUsers)
        {
            connectionIds = OnlineUsers.GetValueOrDefault(userEmail);
        }

        return Task.FromResult(connectionIds);
    }

}