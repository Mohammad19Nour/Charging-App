using System.ComponentModel.DataAnnotations;

namespace ChargingApp.DTOs;

public class AdminRegisterDto
{ 
    
    [Required] public string FirstName { get; set; }
    [Required] public string LastName { get; set; }
    [Required] public string Email { get; set; }
    [Required] [StringLength(10, MinimumLength = 6)]
    public string Password { get; set; }
    [Required] public List<string> Roles { get; set; }
    public string Country { get; set; }
    public string City { get; set; }
    [Required] public string PhoneNumer { get; set; }
}