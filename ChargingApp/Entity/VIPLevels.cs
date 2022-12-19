namespace ChargingApp.Entity;

public class VIPLevels
{
    public int Id { get; set; }
    public int VIP_Level { get; set; }
    public int Discount { get; set; }
    public int MinimumPurchase { get; set; } = 0;
}