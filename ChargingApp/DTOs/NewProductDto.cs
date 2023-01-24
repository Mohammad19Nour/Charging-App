namespace ChargingApp.DTOs;

public class NewProductDto
{
    public string? EnglishName { get; set; }
    public string? ArabicName { get; set; }
    public int MinimumQuantityAllowed { get; set; }
    public IFormFile PhotoFile { get; set; }
    public double Price { get; set; }
}

public class NewProductWithQuantityDto
{
    public ICollection<double> PriceList { get; set; }
    public ICollection<double> QuantityList { get; set; }
}