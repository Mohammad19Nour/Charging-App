using ChargingApp.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace ChargingApp.Data;

public class DataContext : IdentityDbContext<AppUser, AppRole, int, IdentityUserClaim<int>,
    AppUserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
{

    public DataContext(DbContextOptions options) : base(options)
    {
    }

    public DataContext()
    {
        
    }

    public DbSet<DebitHistory> Debits { get; set; }
    public DbSet<VIPLevel> VipLevels { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<RechargeMethod> RechargeMethods { get; set; }
    public DbSet<ChangerAndCompany> ChangerAndCompanies { get; set; }
    public DbSet<PaymentGateway> PaymentGateways { get; set; }
    public DbSet<RechargeCode> RechargeCodes { get; set; }
    public DbSet<Currency> Currencies { get; set; }
    public DbSet<OurAgent> OurAgents { get; set; }
    public DbSet<SpecificPriceForUser> SpecificPriceForUsers { get; set; }
    public DbSet<SpecificBenefitPercent> SpecificBenefit { get; set; }
    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<SliderPhoto> SliderPhotos { get; set; }
    public DbSet<Photo> Photos { get; set; }
    public DbSet<OrderAndPaymentNotification> OrderAndPaymentNotifications { get; set; }
    public DbSet<ApiProduct> ApiProducts { get; set; }
    public DbSet<ApiOrder> ApiOrders { get; set; }
    public DbSet<NotificationHistory> NotificationsHistory { get; set; }
    public DbSet<SupportNumber> SupportNumbers { get; set; }
    public DbSet<HostingSite>HostingSites { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<AppUser>()
            .HasMany(ur => ur.UserRoles)
            .WithOne(u => u.User)
            .HasForeignKey(ur => ur.UserId)
            .IsRequired();

        builder.Entity<AppRole>()
            .HasMany(ur => ur.UserRoles)
            .WithOne(u => u.Role)
            .HasForeignKey(ur => ur.RoleId)
            .IsRequired();
        
        builder.Entity<Favorite>()
            .HasKey(p => new { p.CategoryId, p.UserId });

        var sqlite = Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite";
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                var properties = entityType.ClrType.GetProperties()
                    .Where(p => p.PropertyType == typeof(decimal));

                foreach (var property in properties)
                {
                    if (sqlite)
                        builder.Entity(entityType.Name).Property(property.Name)
                            .HasConversion<double>();
                    else
                        builder.Entity(entityType.Name).Property(property.Name)
                            .HasPrecision(38, 12);
                }
            }
    }
}