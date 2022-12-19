using System.ComponentModel.DataAnnotations;

namespace ChargingApp.DTOs;

public class RegisterDto
{
    [Required] public string Email { get; set; }
    [Required] public string FirstName { get; set; }
    [Required] public string LastName { get; set; }

    [Required]
    [StringLength(10, MinimumLength = 6)]
    public string Password { get; set; }

    public string AccountType { get; set; } = "Normal";

     public string Country { get; set; }
     public string City { get; set; }
    [Required] public string PhoneNumer { get; set; }
}