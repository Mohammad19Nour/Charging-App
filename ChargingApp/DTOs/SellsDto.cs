namespace ChargingApp.DTOs;

public class SellsDto
{
    
    public string Username { get; set; } 
    public string UserEmail { get; set; }
    public string PlayerName { get; set; }
    public string? PlayerId { get; set; }
    public string ProductEnglishName { get; set; }
    public string ProductArabicName { get; set; }
    public double TotalPrice { get; set; }
    public double TotalQuantity { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}