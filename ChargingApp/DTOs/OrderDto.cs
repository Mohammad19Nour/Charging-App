namespace ChargingApp.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public string CreatedAt { get; set; }
    public string ProductName { get; set; }
    public double TotalPrice { get; set; }
    public string PlayerId { get; set; }
    public int Quantity { get; set; }
    public string Status { get; set; }
}

public class NormalOrderDto : OrderDto
{
    public string? TransferNumber { get; set; }
}