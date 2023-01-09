using System.ComponentModel.DataAnnotations;
using EntityFrameworkCore.EncryptColumn.Attribute;
using Microsoft.AspNetCore.Identity;

namespace ChargingApp.Entity;

public class AppUser : IdentityUser<int>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public double Balance { get; set; } = 0;
    public int VIPLevel { get; set; } = 0;
    public double TotalPurchasing { get; set; } = 0;
    public double Debit { get; set; } = 0;
    public double TotalForVIPLevel { get; set; } = 0;
    public ICollection<AppUserRole> UserRoles { get; set; }

}