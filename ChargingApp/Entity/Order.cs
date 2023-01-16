namespace ChargingApp.Entity;

public class Order :BaseEntity
{
    public AppUser? User { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public double TotalPrice { get; set; }
    public Photo? Photo { get; set; }
    public string? PlayerId { get; set; }
    public string PlayerName { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; } = 1;
    public PaymentGateway? PaymentGateway { get; set; }
    public string OrderType { get; set; } = "Normal";
    public string Notes { get; set; }= "Pending";
    public int Status { get; set; } = 0;//0-pending 1-succeed 2-rejected 3-wrong
    public int StatusIfCanceled { get; set; } = 0;
    /*0 not canceled
    1 canceled but not confirmed by admin
    2 canceled and accepted by admin
    3 canceled but rejected by admin
    */
}
