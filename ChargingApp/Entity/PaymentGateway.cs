namespace ChargingApp.Entity;

public class PaymentGateway :BaseEntity
{
    public string EnglishName { get; set; }
    public string ArabicName { get; set; }
    public string BagAddress { get; set; }
}