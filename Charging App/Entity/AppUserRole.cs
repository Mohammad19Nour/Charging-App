using Microsoft.AspNetCore.Identity;

namespace Charging_App.Entity;

public class AppUserRole : IdentityUserRole<int>
{
    public AppUser User { get; set; }

    public AppRole Role { get; set; }
}