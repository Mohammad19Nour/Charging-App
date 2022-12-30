namespace ChargingApp.DTOs;

public class NewPaymentDto
{
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string? Notes{ get; set; }
    public int AddedValue { get; set; }
    public string Username { get; set; }
    
}

public class NewCompanyPaymentDto : NewPaymentDto
{
    public string? SecretNumber { get; set; }
    public string? ReceiptNumber { get; set; }
}
