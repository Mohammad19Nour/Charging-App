namespace ChargingApp.DTOs;

public class ProductToUpdateDto
{
    public string? EnglishName { get; set; }
    public string? ArabicName { get; set; }
    public decimal? Price { get; set; }
    public bool? Available { get; set; }
    public int? MinimumQuantityAllowed { get; set; }
}
public class ProductWithQuantityToUpdateDto
{
    public decimal? Price { get; set; }
    public decimal? Quantity { get; set; }
    public bool? Available { get; set; }
}