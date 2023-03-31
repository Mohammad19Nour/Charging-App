namespace ChargingApp.DTOs;

public class OrderAdminDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    public decimal TotalPrice { get; set; }
    public string? PlayerId { get; set; }
    public decimal Quantity { get; set; } = 1;
    public string OrderType { get; set; } = "Normal";
    public string Notes { get; set; }= "Pending";
}