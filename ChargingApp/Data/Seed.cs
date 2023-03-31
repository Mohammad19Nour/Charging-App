using ChargingApp.Entity;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChargingApp.Data;

public static class Seed
{
    public static async Task SeedRoles(RoleManager<AppRole> roleManager)
    {
        if (await roleManager.Roles.AnyAsync()) return;

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
    }

    public static async Task SeedUsers(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        if (await userManager.Users.AnyAsync()) return;

        var admin = new AppUser
        {
            FirstName = "hamza",
            LastName = "turkea",
            Email = "hamzaturkea@gmail.com",
            UserName = "hamzaturkea@gmail.com",
            City = "Admin",
            Country = "Admin",
            PhoneNumber = "8596539",
            Balance = 10000,
            VIPLevel = 1,
            EmailConfirmed = true,
        };
        await userManager.CreateAsync(admin, "Admin!1");
        await userManager.AddToRolesAsync(admin, new[] { "Admin", "VIP" });

        admin = new AppUser
        {
            FirstName = "hamam",
            LastName = "hamam",
            Email = "hamam@gmail.com",
            UserName = "hamam@gmail.com",
            City = "Admin",
            Country = "Admin",
            PhoneNumber = "8596539",
            Balance = 10000,
            VIPLevel = 1,
            EmailConfirmed = true,
        };
        await userManager.CreateAsync(admin, "Admin!1");
        await userManager.AddToRolesAsync(admin, new[] { "Admin", "VIP" });
    }

    public static async Task SeedVipLevels(DataContext context)
    {
        if (await context.VipLevels.AnyAsync()) return;

        // normal user is considered as vip 0 so don't remove this line
        context.VipLevels.Add(new VIPLevel { BenefitPercent = 20, VipLevel = 0, Photo = new Photo { Url = "http" } });
        context.VipLevels.Add(new VIPLevel
            { BenefitPercent = 10, VipLevel = 1, Purchase = 132, Photo = new Photo { Url = "http1" } });
        context.VipLevels.Add(new VIPLevel
        {
            BenefitPercent = 1, VipLevel = 2, MinimumPurchase = 132, Purchase = 1000,
            Photo = new Photo { Url = "http2" }
        });

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

    public static async Task SeedSites(DataContext context)
    {
        if (await context.HostingSites.AnyAsync()) return;

        context.HostingSites.Add(new HostingSite
        {
            SiteName = "Fast store",
            BaseUrl = "https://api.fast-store.co/client/api",
            Token = "7515d6dd5ae4de7b4e7de94c55aea5a2ff17fa37f45da962"
        });
        context.HostingSites.Add(new HostingSite
        {
            SiteName = "Prince Cash",
            BaseUrl = "https://api.prince-cash.com/client/api",
            Token = "c9fe2310c360010d1349258c9672e98fd8a87adc827a10d4"
        });

        context.HostingSites.Add(new HostingSite
        {
            SiteName = "Speed Card",
            BaseUrl = "https://api.speedcard.vip/client/api",
            Token = "207f93847085daf3c62f1b9dc3990290b12affea9d22afe8"
        });
        context.HostingSites.Add(new HostingSite
        {
            SiteName = "Life Cash",
            BaseUrl = "https://api.life-cash.com/client/api",
            Token = "d1158cc2aaecd9afd6f8bcaca0fad78afca8d9d9c28b97b0"
        });

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

    public static async Task SeedCategories(DataContext context)
    {
        if (await context.Categories.AnyAsync()) return;

        var photo = new Photo { Url = "/testt" };
        context.Categories.Add(new Category
            { EnglishName = "pubg", ArabicName = "arabic", HasSubCategories = true, Photo = photo });
        context.Categories.Add(new Category
        {
            EnglishName = "clash royal", ArabicName = "arabic", HasSubCategories = false,
            Photo = new Photo { Url = "/test" }
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