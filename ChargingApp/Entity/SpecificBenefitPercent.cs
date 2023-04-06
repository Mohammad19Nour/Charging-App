namespace ChargingApp.Entity;

public class SpecificBenefitPercent: BaseEntity
{
    public int ProductId { get; set; }
    public decimal BenefitPercent { get; set; }
}