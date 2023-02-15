namespace ChargingApp.Entity;

public class ApiOrder : BaseEntity
{
    public Order Order { get; set; }
    public int ApiOrderId { get; set; }
    public HostingSite HostingSite { get; set; }
}