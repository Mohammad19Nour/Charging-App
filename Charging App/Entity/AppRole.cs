using Microsoft.AspNetCore.Identity;

namespace Charging_App.Entity;

public class AppRole : IdentityRole<int>
{
    public ICollection<AppUserRole> UserRoles { get; set; }
}