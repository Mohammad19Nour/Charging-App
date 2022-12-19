﻿using Microsoft.AspNetCore.Identity;

namespace ChargingApp.Entity;

public class AppRole : IdentityRole<int>
{
    public ICollection<AppUserRole> UserRoles { get; set; }
}