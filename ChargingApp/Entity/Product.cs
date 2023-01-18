namespace ChargingApp.Entity;

public class Product :BaseEntity
{
    public string? EnglishName { get; set; }
    public string? ArabicName { get; set; }
    public double Price { get; set; }
    public double Quantity { get; set; } = 0;
    public bool CanChooseQuantity { get; set; } = false;
    public bool Available { get; set; } = true;
    public Photo? Photo { get; set; }
    public Category? Category { get; set; }
    public int? CategoryId { get; set; }
    public int MinimumQuantityAllowed { get; set; } = 1;
}