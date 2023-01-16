namespace ChargingApp.Entity;

public class Payment :BaseEntity
{
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string? Notes{ get; set; }
    public double AddedValue { get; set; }
    public Photo? Photo { get; set; }
    public int Status { get; set; } = 0;//0-pending 1-succeed 2-rejected
    public string? Username { get; set; }
    public string PaymentType { get; set; } = "USDT"; // office or company or usdt 
    public string? PaymentAgentEnglishName { get; set; }
    public string? PaymentAgentArabicName { get; set; }
    public AppUser User { get; set; }
}