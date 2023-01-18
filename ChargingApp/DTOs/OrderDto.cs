namespace ChargingApp.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ProductEnglishName { get; set; }
    public string ProductArabicName { get; set; }
    public double TotalPrice { get; set; }
    public string PlayerId { get; set; }
    public double TotalQuantity { get; set; }
    public string Status { get; set; }
    public string Notes { get; set; }
    public string StatusIfCanceled { get; set; }
}

public class NormalOrderDto : OrderDto
{ 
    public string? ReceiptNumberUrl { get; set; }
}