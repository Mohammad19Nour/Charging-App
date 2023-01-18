namespace ChargingApp.DTOs;

public class NewProductDto : ProductInfo
{
    public IFormFile PhotoFile { get; set; }
    public double Price { get; set; }
    public double Quantity { get; set; }
}

public class NewProductWithQuantityDto
{
    public bool CanChooseQuantity { get; set; } = false;
    public ICollection<double> PriceList { get; set; }
    public ICollection<double> QuantityList { get; set; }
}