using System.ComponentModel.DataAnnotations;

namespace ChargingApp.DTOs;

public class NewOrderDto
{
    [Required] public string? PlayerId { get; set; }
    [Required] public int ProductId { get; set; }
    [Required] public string PlayerName { get; set; }
    public decimal Quantity { get; set; } = 1;
}

public class NewNormalOrderDto : NewOrderDto
{
    [Required] public string PaymentGateway { get; set; } //paypal or visa
    [Required] public IFormFile ReceiptPhoto { get; set; }
}