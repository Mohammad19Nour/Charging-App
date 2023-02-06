namespace ChargingApp.Entity;

public class ApiProduct : BaseEntity
{
    public Product Product { get; set; }
    public int ApiProductId { get; set; }
}