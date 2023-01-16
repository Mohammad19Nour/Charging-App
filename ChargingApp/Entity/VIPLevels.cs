namespace ChargingApp.Entity;

public class VIPLevel :BaseEntity
{
    public int VipLevel { get; set; }
    public int BenefitPercent { get; set; }
    public int MinimumPurchase { get; set; } = 0;
}