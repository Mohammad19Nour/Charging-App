namespace ChargingApp.DTOs;

public class PaymentDto
{
    public int Id { get; set; }
    public string CreatedDate { get; set; }
    public string? Notes { get; set; }
    public decimal AddedValue { get; set; }
    public string Status { get; set; }
    public string Username { get; set; }
    public string PaymentType { get; set; }
    public string? ReceiptNumberUrl { get; set; }
}

public class CompanyPaymentDto : PaymentDto
{
    public string? PaymentAgentArabicName { get; set; }
    public string? PaymentAgentEnglishName { get; set; }
}

public class OfficePaymentDto : PaymentDto
{
    public string? PaymentAgentArabicName { get; set; }
    public string? PaymentAgentEnglishName { get; set; }
}