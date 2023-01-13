using System.ComponentModel.DataAnnotations;

namespace ChargingApp.DTOs;

public class CurrencyDto
{
   [Required]  public string Name { get; set; }
   [Required] public double ValuePerDollar { get; set; }
}