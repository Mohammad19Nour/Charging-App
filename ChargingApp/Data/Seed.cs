using ChargingApp.Entity;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChargingApp.Data;

public static class Seed
{
    public static async Task SeedUsers(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        if (await userManager.Users.AnyAsync()) return;

        var roles = new List<AppRole>
        {
            new() { Name = "Admin" },
            new() { Name = "Normal" },
            new() { Name = "VIP" },
            new() { Name = "Admin_1" },
            new() { Name = "Admin_2" },
            new() { Name = "AdvancedEmployee" },
            new() { Name = "NormalEmployee" },
        };

        foreach (var role in roles)
        {
            await roleManager.CreateAsync(role);
        }

        var user = new AppUser
        {
            UserName = "mm@d.comwd",
            Email = "mm@d.comwd",
            VIPLevel = 1,
            FirstName = "ka",
            LastName = "po",
            PhoneNumber = "369369",
            Country = "ha",
            EmailConfirmed = true,
            Balance = 10000
        };
        await userManager.CreateAsync(user, "Pa$w0rs");
        await userManager.AddToRoleAsync(user, "VIP");

        user = new AppUser
        {
            UserName = "mm@d.com",
            Email = "mm@d.com",
            VIPLevel = 1,
            FirstName = "ka",
            LastName = "po",
            PhoneNumber = "369369",
            Country = "ha",
            Balance = 10000,
            EmailConfirmed = true
        };
        await userManager.CreateAsync(user, "Pa$w0rs");
        await userManager.AddToRoleAsync(user, "VIP");
        user = new AppUser
        {
            UserName = "yy@d.com",
            Email = "yy@d.com",
            FirstName = "ka",
            LastName = "po",
            PhoneNumber = "369369",
            Country = "ha",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(user, "Pa$w0rs");
        await userManager.AddToRoleAsync(user, "Normal");

        user = new AppUser
        {
            UserName = "oo@d.com",
            Email = "oo@d.com",
            VIPLevel = 1,
            FirstName = "ka",
            LastName = "po",
            PhoneNumber = "369369",
            Country = "ha",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(user, "Pa$w0rs");
        await userManager.AddToRoleAsync(user, "Normal");

        user = new AppUser
        {
            UserName = "kk@d.com",
            Email = "kk@d.com",
            VIPLevel = 1,
            FirstName = "ka",
            LastName = "po",
            PhoneNumber = "369369",
            Country = "ha",
            EmailConfirmed = true,
            Balance = 10000
        };
        await userManager.CreateAsync(user, "Pa$w0rs");
        await userManager.AddToRolesAsync(user, new[] { "Admin_1","VIP" });

        var admin = new AppUser
        {
            FirstName = "Admin",
            LastName = "Admin",
            Email = "moh@gmail.com",
            UserName = "moh@gmail.com",
            Balance = 10000,
            VIPLevel = 1,
            EmailConfirmed = true,
        };
        await userManager.CreateAsync(admin, "Admin!1");
        await userManager.AddToRolesAsync(admin, new[] { "Admin" });
    }

    public static async Task SeedVipLevels(DataContext context)
    {
        if (await context.VipLevels.AnyAsync()) return;

        context.VipLevels.Add(new VIPLevel { BenefitPercent = 30, VipLevel = 0 });
        context.VipLevels.Add(new VIPLevel { BenefitPercent = 20, VipLevel = 1 });
        context.VipLevels.Add(new VIPLevel { BenefitPercent = 15, VipLevel = 2, MinimumPurchase = 1000 });
        context.VipLevels.Add(new VIPLevel { BenefitPercent = 10, VipLevel = 3, MinimumPurchase = 2000 });
        context.VipLevels.Add(new VIPLevel { BenefitPercent = 5, VipLevel = 4, MinimumPurchase = 3000 });

        await context.SaveChangesAsync();
    }

    public static async Task SeedCategories(DataContext context)
    {
        if (await context.Categories.AnyAsync()) return;

        var photo = new Photo { Url = "ffff" };
        context.Categories.Add(new Category
            { EnglishName = "pubg", ArabicName = "arabic", HasSubCategories = true, Photo = photo });
        context.Categories.Add(new Category
        {
            EnglishName = "clash royal", ArabicName = "arabic", HasSubCategories = false, Photo = photo
        });
        await context.SaveChangesAsync();
    }

    public static async Task SeedProducts(DataContext context)
    {
        if (await context.Products.AnyAsync()) return;

        context.Products.Add(new
            Product
            {
                Price = 60,
                ArabicName = "pubg 50 card",
                EnglishName = "pubg 50 card",
                CategoryId = 1
            });

        context.Products.Add(new
            Product
            {
                Price = 120,
                ArabicName = "pubg 100 card",
                EnglishName = "pubg 100 card",
                CategoryId = 1
            });

        var product = new Product
        {
            Price = 100,
            Quantity = 70,
            ArabicName = "pubg 100 card",
            EnglishName = "pubg 100 card",
            CanChooseQuantity = true,
            CategoryId = 2
        };
        context.Products.Add(product);
        product = new Product
        {
            Price = 200,
            Quantity = 90,
            ArabicName = "pubg 100 card",
            EnglishName = "pubg 100 card",
            CanChooseQuantity = true,
            CategoryId = 2
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();
    }

    public static async Task SeedPayments(DataContext context)
    {
        if (await context.PaymentGateways.AnyAsync()) return;
        context.PaymentGateways.Add(new PaymentGateway
        {
            EnglishName = "lord",
            ArabicName = "اللورد للحوالات المالية",
            BagAddress = "2654jhjh"
        });
        context.PaymentGateways.Add(new PaymentGateway
        {
            EnglishName = "USDT",
            ArabicName = "USDT",
            BagAddress = "hku5416"
        });
        context.PaymentGateways.Add(new PaymentGateway
        {
            EnglishName = "Payeer",
            ArabicName = "Payeer",
            BagAddress = "lscwlncwlc"
        });
        context.PaymentGateways.Add(new PaymentGateway
        {
            EnglishName = "Binance",
            ArabicName = "Binance",
            BagAddress = "cdncwlkcnlscm"
        });

        await context.SaveChangesAsync();
    }

    public static async Task SeedPaymentMethods(DataContext context)
    {
        if (await context.RechargeMethods.AnyAsync()) return;

        context.RechargeMethods.Add(new RechargeMethod { ArabicName = "شركات التحويل", EnglishName = "Companies" });
        context.RechargeMethods.Add(new RechargeMethod { ArabicName = "مكاتب الصرافين", EnglishName = "Offices" });
        await context.SaveChangesAsync();
    }

    public static async Task SeedCompanies(DataContext context)
    {
        if (await context.ChangerAndCompanies.AnyAsync()) return;

        var com = await context.RechargeMethods
            .FirstOrDefaultAsync(x => x.Id == 2);
        com.ChangerAndCompanies.Add(new ChangerAndCompany
        {
            EnglishName = "eng",
            ArabicName = "arb"
        });
        com.ChangerAndCompanies.Add(new ChangerAndCompany
        {
            EnglishName = "popo",
            ArabicName = "عربي"
        });


        var cc = await context.RechargeMethods
            .FirstOrDefaultAsync(x => x.Id == 1);
        cc.ChangerAndCompanies.Add(new ChangerAndCompany
        {
            EnglishName = "eng",
            ArabicName = "اسم عربي"
        });
        await context.SaveChangesAsync();
    }

    public static async Task SeedCurrency(DataContext context)
    {
        if (await context.Currencies.AnyAsync()) return;

        context.Currencies.Add(new Currency
        {
            Name = "Turkish",
            ValuePerDollar = 20,
        });
        context.Currencies.Add(new Currency
        {
            Name = "Syrian",
            ValuePerDollar = 6000,
        });
        await context.SaveChangesAsync();
    }

    public static async Task SeedOurAgents(DataContext context)
    {
        if (await context.OurAgents.AnyAsync()) return;

        context.OurAgents.Add(new OurAgent
        {
            EnglishName = "dss",
            ArabicName = "عربي",
            City = "homs",
            PhoneNumber = "656566451"
        });
        context.OurAgents.Add(new OurAgent
        {
            EnglishName = "dss",
            ArabicName = "عربي",
            City = "homs",
            PhoneNumber = "656566451"
        });

        await context.SaveChangesAsync();
    }
}