namespace ChargingApp.DTOs;

public class WalletDto
{
    public double TurkishBalance { get; set; }
    public double SyrianBalance { get; set; }
    public double DollarBalance { get; set; }
    
    public double TurkishTotalPurchase { get; set; }
    public double SyrianTotalPurchase { get; set; }
    public double DollarTotalPurchase { get; set; }

    public double TurkishDebit { get; set; }
    public double SyrianDebit { get; set; }
    public double DollarDebit { get; set; }

    public double DollarVIPPurchase { get; set; }
    public double SurianVIPPurchase { get; set; }
    public double TurkishVIPPurchase { get; set; }

}