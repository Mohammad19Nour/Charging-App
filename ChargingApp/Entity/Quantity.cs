namespace ChargingApp.Entity;

public class Quantity :BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Value { get; set; }
    public int Price { get; set; }
}