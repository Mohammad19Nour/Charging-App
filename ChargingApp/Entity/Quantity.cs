namespace ChargingApp.Entity;

public class Quantity
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Value { get; set; }
}