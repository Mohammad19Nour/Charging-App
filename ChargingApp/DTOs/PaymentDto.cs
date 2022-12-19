namespace ChargingApp.DTOs;

public class PaymentDto
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? Notes{ get; set; }
    public int AddedValue { get; set; }
    public string? SecretNumber { get; set; }
    public string? ReceiptNumber { get; set; }
    public bool Aproved { get; set; }
    public bool Scucced { get; set; }
    public string Username { get; set; }
    public string? PaymentAgent { get; set; }
    public string PaymentType { get; set; }
}