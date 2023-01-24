using ChargingApp.Entity;

namespace ChargingApp.DTOs;

public class PaymentAdminDto
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; } 
    public string? Notes{ get; set; }
    public double AddedValue { get; set; }
    public string Photo { get; set; }
    public string? Username { get; set; }
    public string PaymentType { get; set; }// office or company or usdt 
    public string? PaymentAgentEnglishName { get; set; }
    public string? PaymentAgentArabicName { get; set; }
    public string? Email { get; set; }

}