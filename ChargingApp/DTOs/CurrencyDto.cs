using System.ComponentModel.DataAnnotations;

namespace ChargingApp.DTOs;

public class CurrencyDto
{
   [Required]  public string Name { get; set; }
   [Required] public decimal ValuePerDollar { get; set; }
}