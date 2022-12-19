using System.ComponentModel.DataAnnotations;

namespace ChargingApp.Entity;

public class RechargeCode
{
    [Key]
    public string Code { get; set; }
    public int Value { get; set; }
    public bool Istaked { get; set; } = false;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? TakedTime { get; set; }
    public AppUser? User { get; set; }
}