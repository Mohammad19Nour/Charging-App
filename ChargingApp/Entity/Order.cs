namespace ChargingApp.Entity;

public class Order
{
    public int Id { get; set; }
    public AppUser? User { get; set; }
    public int UserId { get; set; }
    public string? TransferNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public double TotalPrice { get; set; }
    public string? PlayerId { get; set; }
    public Product? Product { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
    public bool Succeed { get; set; } = false;
    public bool Approved { get; set; } = false;
    public PaymentGateway? PaymentGateway { get; set; }
    public string OrderType { get; set; } = "Normal";
}