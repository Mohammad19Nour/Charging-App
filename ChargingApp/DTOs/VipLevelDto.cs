namespace ChargingApp.DTOs;

public class VipLevelInfo
{
    public int BenefitPercent { get; set; }
    public int MinimumPurchase { get; set; } = 0;
}

public class VipLevelDto
{
    public int VipLevel { get; set; }
    public int MinimumPurchase { get; set; }
}