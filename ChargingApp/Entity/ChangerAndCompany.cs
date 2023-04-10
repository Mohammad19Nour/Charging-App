using System.ComponentModel.DataAnnotations.Schema;

namespace ChargingApp.Entity;
public class ChangerAndCompany :BaseEntity
{
    public RechargeMethod RechargeMethodMethod { get; set; }
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    public Photo Photo { get; set; }
}