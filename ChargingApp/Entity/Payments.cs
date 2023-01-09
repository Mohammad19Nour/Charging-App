namespace ChargingApp.Entity;

public class Payment :BaseEntity
{
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string? Notes{ get; set; }
    public double AddedValue { get; set; }
    public Photo? Photo { get; set; }
    public bool Checked { get; set; } = false;
    public bool Succeed { get; set; } = false;
    public string? Username { get; set; }
    public string PaymentType { get; set; } = "USDT"; // office or company or usdt 
    public string? PaymentAgentEnglishName { get; set; }
    public string? PaymentAgentArabicName { get; set; }
    public AppUser User { get; set; }
}