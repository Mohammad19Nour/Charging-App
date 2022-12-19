using System.ComponentModel.DataAnnotations.Schema;

namespace ChargingApp.Entity;
public class ChangerAndCompany
{
    public int Id { get; set; }
    public RechargeMethod RechargeMethodMethod { get; set; }
    public int PaymentMethodId { get; set; }
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
}