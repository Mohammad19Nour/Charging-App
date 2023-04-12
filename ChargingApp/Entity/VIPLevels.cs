namespace ChargingApp.Entity;

public class VIPLevel :BaseEntity
{
    public int VipLevel { get; set; }
    public string EnglishName { get; set; }
    public string ArabicName { get; set; }
    public decimal BenefitPercent { get; set; }
    public decimal MinimumPurchase { get; set; } = 0; // cumulative sum for purchasing of all vip levels less than current one
    public decimal Purchase { get; set; } = 0; // total purchasing should one take to move to the next vip level
    public Photo Photo { get; set; }
}