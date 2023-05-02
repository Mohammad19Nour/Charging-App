namespace ChargingApp.DTOs;

public class NewProductDto
{
    public string? EnglishName { get; set; }
    public string? ArabicName { get; set; }
    public int MinimumQuantityAllowed { get; set; } = 1;
    public IFormFile PhotoFile { get; set; }
    public decimal Price { get; set; }
}

public class NewProductWithQuantityDto
{
    public ICollection<decimal> PriceList { get; set; }
    public ICollection<decimal> QuantityList { get; set; }
}