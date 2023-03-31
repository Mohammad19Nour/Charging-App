namespace ChargingApp.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ProductEnglishName { get; set; }
    public string ProductArabicName { get; set; }
    public decimal TotalPrice { get; set; }
    public string PlayerId { get; set; }
    public decimal TotalQuantity { get; set; }
    public string Status { get; set; }
    public string Notes { get; set; }
    public string StatusIfCanceled { get; set; }
}

public class NormalOrderDto : OrderDto
{
    public string GatewayArabicName { get; set; }
    public string GatewayEnglishName { get; set; }
    public string? ReceiptNumberUrl { get; set; }
}