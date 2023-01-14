namespace ChargingApp.DTOs;

public class VipLevelInfo
{
    public int BenefitPercent { get; set; }
    public int MinimumPurchase { get; set; } = 0;
}
public class VipLevelDto:VipLevelInfo
{
    public int VIP_Level { get; set; }
}