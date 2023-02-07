namespace ChargingApp.DTOs;

public class ProductInfo
{
    public string? EnglishName { get; set; }
    public string? ArabicName { get; set; }
    
    public bool Available { get; set; }
    public bool CanChooseQuantity { get; set; } = false;
    public int MinimumQuantityAllowed { get; set; }
}

public class ProductDto : ProductInfo
{
    public int Id { get; set; }
    public string Photo { get; set; }
    public double TurkishPrice { get; set; }
    public double SyrianPrice { get; set; }
    public double Price { get; set; }
    public double Quantity { get; set; }
}
