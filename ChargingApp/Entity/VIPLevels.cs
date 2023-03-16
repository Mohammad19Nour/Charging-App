namespace ChargingApp.Entity;

public class VIPLevel :BaseEntity
{
    public int VipLevel { get; set; }
    public double BenefitPercent { get; set; }
    public double MinimumPurchase { get; set; } = 0; // cumulative sum for purchasing of all vip levels less than current one
    public double Purchase { get; set; } = 0; // total purchasing should one take to move to the next vip level
}