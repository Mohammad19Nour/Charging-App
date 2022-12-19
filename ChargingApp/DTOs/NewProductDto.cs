namespace ChargingApp.DTOs;

public class NewProductDto
{
    public double SellingPrice { get; set; }
    public double OrignalPrice { get; set; }
    public string? EnglishName { get; set; }
    public string? ArabicName{ get; set; }
    public bool CanChooseQuantity { get; set; }
    public ICollection<int>? AvailableQuantities { get; set; } = new List<int>();
    public IFormFile PhotoFile { get; set; }
    public int MinimumQuantityAllowed { get; set; }
}