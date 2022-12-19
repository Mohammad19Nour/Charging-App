namespace ChargingApp.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public double Price { get; set; }
    public string? Photo { get; set; }
    public string? EnglishName { get; set; }
    public string? ArabicName{ get; set; }
   // public int Value { get; set; }
   public bool CanChooseQuantity { get; set; } = false;
    public ICollection<int>? AvailableQuantities { get; set; }
} 