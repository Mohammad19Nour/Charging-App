namespace ChargingApp.DTOs;

public class WalletDto
{
    public decimal TurkishBalance { get; set; }
    public decimal SyrianBalance { get; set; }
    public decimal DollarBalance { get; set; }
    
    public decimal TurkishTotalPurchase { get; set; }
    public decimal SyrianTotalPurchase { get; set; }
    public decimal DollarTotalPurchase { get; set; }

    public decimal TurkishDebit { get; set; }
    public decimal SyrianDebit { get; set; }
    public decimal DollarDebit { get; set; }

    public decimal DollarVIPPurchase { get; set; }
    public decimal SurianVIPPurchase { get; set; }
    public decimal TurkishVIPPurchase { get; set; }

}