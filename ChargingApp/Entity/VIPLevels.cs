namespace ChargingApp.Entity;

public class VIPLevels :BaseEntity
{
    public int VIP_Level { get; set; }
    public int BenefitPercent { get; set; }
    public int MinimumPurchase { get; set; } = 0;
}