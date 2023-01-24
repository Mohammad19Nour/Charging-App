namespace ChargingApp.DTOs;

public class ProductToUpdateDto
{
    public string? EnglishName { get; set; }
    public string? ArabicName { get; set; }
    public double Price { get; set; }
    public bool Available { get; set; }
    public int MinimumQuantityAllowed { get; set; }
}
public class ProductWithQuantityToUpdateDto
{
    public double Price { get; set; }
    public double Quantity { get; set; }
    public bool Available { get; set; }
}