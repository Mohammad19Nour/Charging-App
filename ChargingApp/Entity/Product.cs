namespace ChargingApp.Entity;

public class Product :BaseEntity
{
    public double Price { get; set; }
    public double OriginalPrice { get; set; }
    public string? EnglishName { get; set; }
    public string? ArabicName { get; set; }
    public string? EnglishDetails { get; set; }
    public string? ArabicDetails { get; set; }
    public bool CanChooseQuantity { get; set; } = false;
    public bool Available { get; set; } = true;
    public ICollection<Quantity> AvailableQuantities { get; set; } = new List<Quantity>();
    public Photo? Photo { get; set; }
    public Category? Category { get; set; }
    public int? CategoryId { get; set; }
    public int MinimumQuantityAllowed { get; set; } = 1;
}