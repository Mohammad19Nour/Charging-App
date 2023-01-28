namespace ChargingApp.DTOs;

public class PendingOrderDto
{
    public int Id { get; set; }
    public double TotalPrice { get; set; }
    public string PlayerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public double Quantity { get; set; } = 1;
    public string ProductEnglishName { get; set; }
    public string ProductArabicName { get; set; }
    public string UserName { get; set; }
    public string OrderType { get; set; } = "Normal";
    public string Photo { get; set; }
    public string Status { get; set; } = "Waiting";
}