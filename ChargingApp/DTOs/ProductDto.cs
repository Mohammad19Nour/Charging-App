namespace ChargingApp.DTOs;

public class ProductInfo
{
    public string? EnglishName { get; set; }
    public string? ArabicName { get; set; }
    public double Price { get; set; }
}

public class ProductDto : ProductInfo
{
    public int Id { get; set; }
    public string? Photo { get; set; }

    public bool Available { get; set; }
    public bool CanChooseQuantity { get; set; } = false;
    public ICollection<int>? AvailableQuantities { get; set; }
    public int MinimumQuantityAllowed { get; set; }
}

public class ProductToUpdateDto : ProductInfo
{
    public double OriginalPrice { get; set; }
}

public class NewProductDto : ProductInfo
{
    public double OriginalPrice { get; set; }
    public bool CanChooseQuantity { get; set; }
    public ICollection<int>? AvailableQuantities { get; set; } = new List<int>();
    public IFormFile PhotoFile { get; set; }
    public int MinimumQuantityAllowed { get; set; }
}