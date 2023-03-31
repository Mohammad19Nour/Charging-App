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
    public decimal TurkishPrice { get; set; }
    public decimal SyrianPrice { get; set; }
    public decimal Price { get; set; }
    public decimal Quantity { get; set; }
}
