namespace ChargingApp.Entity;

public class VIPLevel :BaseEntity
{
    public int VipLevel { get; set; }
    public double BenefitPercent { get; set; }
    public double MinimumPurchase { get; set; } = 0;
}