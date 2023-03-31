namespace ChargingApp.DTOs;

public class NewSpecificPriceForUserDto
{
    public string Email { get; set; }
    public int ProductId { get; set; }
    public int VipLevel { get; set; }
    public decimal ProductPrice { get; set; }
}