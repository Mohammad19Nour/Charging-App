using Microsoft.AspNetCore.Identity;

namespace Charging_App.Entity;

public class AppUser : IdentityUser<int>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int CountryCode { get; set; }
    public string? City { get; set; }
    public string AccountType { get; set; } = "Normal";
    public int Balance { get; set; } = 0;
    public int VIPLevel { get; set; } = 0;
    public ICollection<AppUserRole> UserRoles { get; set; }

}