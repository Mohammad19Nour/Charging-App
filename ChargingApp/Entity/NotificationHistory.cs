namespace ChargingApp.Entity;

public class NotificationHistory : BaseEntity
{
    public AppUser User { get; set; }
    public string ArabicDetails { get; set; }
    public string EnglishDetails { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
}