namespace ChargingApp.DTOs;

public class PaymentDto
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? Notes { get; set; }
    public int AddedValue { get; set; }

    public bool Approved { get; set; }
    public bool Succeed { get; set; }
    public string Username { get; set; }
    public string PaymentType { get; set; }
}

public class CompanyPaymentDto : PaymentDto
{
    public string? SecretNumber { get; set; }
    public string? ReceiptNumber { get; set; }
    public string? PaymentAgent { get; set; }
}

public class OfficePaymentDto : PaymentDto
{
    public string? PaymentAgent { get; set; }
}