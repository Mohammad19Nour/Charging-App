namespace ChargingApp.DTOs;

public class PaymentGatewayDto
{
    public int Id { get; set; }
    public string EnglishName { get; set; }
    public string ArabicName { get; set; }
    public string BagAddress { get; set; }
    
    public string Photo { get; set; }
}