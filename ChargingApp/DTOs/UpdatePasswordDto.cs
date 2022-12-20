using System.ComponentModel.DataAnnotations;

namespace ChargingApp.DTOs;

public class UpdatePasswordDto
{
  [Required]  public string OldPassword { get; set; }
   [Required]  [StringLength(10, MinimumLength = 6)]
   public string NewPassword { get; set; }
}