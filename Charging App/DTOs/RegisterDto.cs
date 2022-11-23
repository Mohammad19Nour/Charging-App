using System.ComponentModel.DataAnnotations;

namespace Charging_App.DTOs;

public class RegisterDto
{
    [Required] public string Email { get; set; }
    [Required] public string FirstName { get; set; }
    [Required] public string LastName { get; set; }

    [Required]
    [StringLength(8, MinimumLength = 6)]
    public string Password { get; set; }

    [Required] public string AccountType { get; set; } = "Normal";
    [Required] public int CountryCode { get; set; }
    [Required] public string City { get; set; }
}