namespace ChargingApp.Entity;

public class Payment
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public string? Notes{ get; set; }
    public int AddedValue { get; set; }
    public string? SecretNumber { get; set; }
    public string? ReceiptNumber { get; set; }
    public bool Aproved { get; set; } = false;
    public bool Scucced { get; set; } = false;
    public string Username { get; set; }
    public string PaymentType { get; set; } = "USDT"; // office or company or usdt 
    public string? PaymentAgent { get; set; }
    public AppUser User { get; set; }
}