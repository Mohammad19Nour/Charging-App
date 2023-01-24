namespace ChargingApp.Entity;

public class OrderAndPaymentNotification : BaseEntity
{
    public AppUser User { get; set; }
    public Order? Order { get; set; }
    public Payment? Payment { get; set; }
}