using System.Security.Claims;
using System.Text;
using ChargingApp.Data;
using ChargingApp.Entity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace ChargingApp.Extentions;

public static class IdentityServiceExtensions
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services,
        IConfiguration config)
    {
        services.AddAuthorization(opt =>
        {
            opt.AddPolicy("Required_Administrator_Role", policy => policy.RequireRole("Admin"));
            opt.AddPolicy("Required_Admin1_Role", policy => policy.RequireRole("Admin_1", "Admin"));
            opt.AddPolicy("Required_Admin2_Role", policy => policy.RequireRole("Admin_2", "Admin"));
            opt.AddPolicy("Required_NormalEmployee_Role", policy => policy.RequireRole("NormalEmployee"));
            opt.AddPolicy("Required_AdvancedEmployee_Role", policy => policy.RequireRole("AdvancedEmployee"));
            opt.AddPolicy("Required_Normal_Role",
                policy => policy.RequireRole("Normal", "Admin", "Admin_1", "Admin_2", "AdvancedEmployee",
                    "NormalEmployee"));
            opt.AddPolicy("Required_VIP_Role",
                policy => policy.RequireRole("VIP", "Admin", "Admin_1", "Admin_2", "AdvancedEmployee",
                    "NormalEmployee"));
            opt.AddPolicy("Required_AnyAdmin_Role", policy =>
                policy.RequireRole("Admin", "Admin_1", "Admin_2", "AdvancedEmployee", "NormalEmployee"));
            opt.AddPolicy("Required_AllAdminExceptNormal_Role", policy =>
                policy.RequireRole("Admin", "Admin_1", "Admin_2", "AdvancedEmployee"));
            opt.AddPolicy("Required_Admins_Role", policy =>
                policy.RequireRole("Admin", "Admin_1", "Admin_2"));
            opt.AddPolicy("Required_Admin1-Adv_Role", policy =>
                policy.RequireRole("Admin", "Admin_1", "AdvancedEmployee"));
        });

        services.AddIdentityCore<AppUser>(
                opt =>
                {
                    opt.User.RequireUniqueEmail = true;
                    opt.Password.RequireDigit = false;
                    opt.Password.RequireNonAlphanumeric = false;
                    opt.Password.RequireUppercase = false;
                })
            .AddRoles<AppRole>()
            .AddRoleManager<RoleManager<AppRole>>()
            .AddSignInManager<SignInManager<AppUser>>()
            .AddRoleValidator<RoleValidator<AppRole>>()
            .AddEntityFrameworkStores<DataContext>()
            .AddDefaultTokenProviders();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"])),
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateLifetime = true,
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        Console.WriteLine("884\n\n\n\n");
                        var userManager = context.HttpContext.RequestServices
                            .GetRequiredService<UserManager<AppUser>>();
                        var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
                        if (claimsIdentity is null) context.Fail("Unauthorized");

                        var email = claimsIdentity?.FindFirst(ClaimTypes.Email)?.Value;

                        if (email is null)
                        {
                            context.Fail("Unauthorized");
                        }

                        var user = await userManager.FindByEmailAsync(email);
                        if (user == null)
                        {
                            context.Fail("Unauthorized");
                        }

                        var roles = await userManager.GetRolesAsync(user);

                        // Add the custom claim to the bearer token
                        var identity = new ClaimsIdentity();
                        identity.AddClaims(roles.Select(r => new Claim(ClaimTypes.Role, r)));
                        context.Principal?.AddIdentity(identity);
                    },
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });
        return services;
    }
}