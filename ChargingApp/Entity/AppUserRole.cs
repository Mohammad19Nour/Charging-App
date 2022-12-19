using Microsoft.AspNetCore.Identity;

namespace ChargingApp.Entity;

public class AppUserRole : IdentityUserRole<int>
{
    public AppUser User { get; set; }

    public AppRole Role { get; set; }
}