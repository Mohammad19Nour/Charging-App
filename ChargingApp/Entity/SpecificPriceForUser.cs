namespace ChargingApp.Entity;

public class SpecificPriceForUser : BaseEntity
{
    public AppUser User { get; set; }
    public int ProductId { get; set; }
    public int VipLevel { get; set; }
    public double ProductPrice { get; set; }
}