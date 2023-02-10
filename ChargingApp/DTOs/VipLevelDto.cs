namespace ChargingApp.DTOs;

public class VipLevelInfo
{
    public double? BenefitPercent { get; set; }
    public double? MinimumPurchase { get; set; }
}

public class VipLevelDto
{
    public int VipLevel { get; set; }
    public double MinimumPurchase { get; set; }
}
public class NewVipLevel
{
    public int VipLevel { get; set; }
    public double BenefitPercent { get; set; }
    public double MinimumPurchase { get; set; } = 0;
}