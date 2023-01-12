namespace ChargingApp.Entity;

public class BenefitPercentInSpecificVilLevel : BaseEntity
{
    public int ProductId { get; set; }
    public int VipLevel { get; set; }
    public double BenefitPercent { get; set; }
}