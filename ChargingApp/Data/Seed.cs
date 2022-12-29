using ChargingApp.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChargingApp.Data;

public static class Seed
{

    public static async Task SeedUsers(UserManager<AppUser> userManager , RoleManager<AppRole>roleManager)
    {
        if (await userManager.Users.AnyAsync()) return;

        var roles = new List<AppRole>
        {
            new() {Name = "Admin"},
            new() {Name = "Member"},
        };

        foreach (var role in roles)
        {
            await roleManager.CreateAsync(role);
        }

        var  user = new AppUser
        {
            UserName = "mm@d.comwd",
            Email = "mm@d.comwd",
            VIPLevel = 1,
            FirstName = "ka",
            LastName = "po",
            PhoneNumber = "369369",
            Country = "ha"
        };
        await userManager.CreateAsync(user, "Pa$w0rs");
         user = new AppUser
        {
            UserName = "mm@d.com",
            Email = "mm@d.com",
            VIPLevel = 1,
            FirstName = "ka",
            LastName = "po",
            PhoneNumber = "369369",
            Country = "ha",
        };
        await userManager.CreateAsync(user, "Pa$w0rs");
        await userManager.AddToRoleAsync(user,"Member");
        user = new AppUser
        {
            UserName = "yy@d.com",
            Email = "yy@d.com",
            VIPLevel = 2,
            FirstName = "ka",
            LastName = "po",
            PhoneNumber = "369369",
            Country = "ha"
        };
        await userManager.CreateAsync(user, "Pa$w0rs");
        await userManager.AddToRoleAsync(user,"Member");
        
        user = new AppUser
        {
            UserName = "oo@d.com",
            Email = "oo@d.com",
            VIPLevel = 1,
            FirstName = "ka",
            LastName = "po",
            PhoneNumber = "369369",
            Country = "ha"
        };
        await userManager.CreateAsync(user, "Pa$w0rs");
        await userManager.AddToRoleAsync(user,"Member");
        
        user = new AppUser
        {
            UserName = "kk@d.com",
            Email = "kk@d.com",
            VIPLevel = 4,
            FirstName = "ka",
            LastName = "po",
            PhoneNumber = "369369",
            Country = "ha"
        };
        await userManager.CreateAsync(user, "Pa$w0rs");
        await userManager.AddToRoleAsync(user,"Member");
      
        var admin = new AppUser { FirstName = "Admin" , LastName = "Admin" , 
            Email = "moh@gmail.com" , UserName = "moh@gmail.com"};
        await userManager.CreateAsync(admin, "Admin!1");
        await userManager.AddToRolesAsync(admin , new []{"Admin"});

    }

    public static async Task SeedVipLevels(DataContext context)
    {
        if (await context.VipLevels.AnyAsync()) return;

        context.VipLevels.Add(new VIPLevels{Discount = 0 , VIP_Level = 0 });
        context.VipLevels.Add(new VIPLevels{Discount = 10 , VIP_Level = 1});
        context.VipLevels.Add(new VIPLevels{Discount = 20 , VIP_Level = 2 , MinimumPurchase = 1000});
        context.VipLevels.Add(new VIPLevels{Discount = 30 , VIP_Level = 3 , MinimumPurchase = 2000});
        context.VipLevels.Add(new VIPLevels{Discount = 40 , VIP_Level = 4,MinimumPurchase = 3000});

        await context.SaveChangesAsync();
    }

    public static async Task SeedCategories(DataContext context)
    {
        if (await context.Categories.AnyAsync()) return;

        context.Categories.Add(new Category {EnglishName = "pubg" , ArabicName = "arabic",HasSubCategories = true});
        context.Categories.Add(new Category
        {
            EnglishName = "clash royal",ArabicName = "arabic",HasSubCategories = false
        });
        await context.SaveChangesAsync();
        
    }
    public static async Task SeedProducts(DataContext context)
    {
        if (await context.Products.AnyAsync()) return;

        context.Products.Add(new
            Product
            {
                OriginalPrice = 50,
                Price = 60,
                ArabicName = "pubg 50 card",
                EnglishName = "pubg 50 card",
                ArabicDetails = "pubg 50 card",
                EnglishDetails = "pubg 50 card",
                CategoryId = 1
            });
        
        context.Products.Add(new
            Product
            {
                OriginalPrice = 100,
                Price = 120,
                ArabicName = "pubg 100 card",
                EnglishName = "pubg 100 card",
                ArabicDetails = "pubg 100 card",
                EnglishDetails = "pubg 100 card",
                CategoryId = 1
            });

        var product = new Product
            {
                OriginalPrice = 100,
                Price = 120,
                ArabicName = "pubg 100 card",
                EnglishName = "pubg 100 card",
                ArabicDetails = "pubg 100 card",
                EnglishDetails = "pubg 100 card",
                CanChooseQuantity = true,
                CategoryId = 2
            };
        context.Products.Add(product);
        await context.SaveChangesAsync();
        var quantities = new List<Quantity>
        {
            new(){Product = product,Value = 10},
            new(){Product = product,Value = 70},
            new(){Product = product,Value = 90},
        };
        context.Quantities.AddRange(quantities);
        product.AvailableQuantities = quantities;
        await context.SaveChangesAsync();
    }

    public static async Task SeedPayments(DataContext context)
    {
       
        if (await context.PaymentGateways.AnyAsync()) return;
        context.PaymentGateways.Add(new PaymentGateway
        {
            Name = "PayPal",
            BagAddress = "2654jhjh"
        });context.PaymentGateways.Add(new PaymentGateway
        {
            Name = "VisaCard",
            BagAddress = "hku5416"
        });

/*
        context.PenddingOrders.Add(new PenddingOrder { UserId = 1,
            
            });
        
        context.PenddingOrders.Add(new PenddingOrder { UserId = 1 });
        context.PenddingOrders.Add(new PenddingOrder { UserId = 2, 
            Products = new List<OrderProductPendding>
            {
                new OrderProductPendding
                {
                    OrderId = 3,
                    ProdudctId = 1
                }
            }
            
        });*/
        await context.SaveChangesAsync();
        
    }
    public static async Task SeedPaymentMethods(DataContext context)
    {
        if (await context.RechargeMethods.AnyAsync()) return;

        context.RechargeMethods.Add(new RechargeMethod {ArabicName = "USDT" , EnglishName = "USDT"});
        context.RechargeMethods.Add(new RechargeMethod {ArabicName = "USDT" , EnglishName = "Companies"});
        context.RechargeMethods.Add(new RechargeMethod {ArabicName = "USDT" , EnglishName = "Changers"});
        await context.SaveChangesAsync();

       /* PaymentMethods met = await context.PaymentMethods.FirstOrDefaultAsync(x => x.Id == 2);
        met.Providers.Add(new ChangerAndCompany
        {
            EnglishName = "company1"
        });
        met.Providers.Add(new ChangerAndCompany
        {
            EnglishName = "company2"
        });met.Providers.Add(new ChangerAndCompany
        {
            EnglishName = "company3"
        });
        
        met = await context.PaymentMethods.FirstOrDefaultAsync(x => x.Id == 3);
        met.Providers.Add(new ChangerAndCompany
        {
            EnglishName = "office1"
        });
        met.Providers.Add(new ChangerAndCompany
        {
            EnglishName = "office2"
        });met.Providers.Add(new ChangerAndCompany
        {
            EnglishName = "office3"
        });
     await context.SaveChangesAsync();*/

    }

    public static async Task SeedCompanies(DataContext context)
    {
        if (await context.ChangerAndCompanies.AnyAsync()) return;

        var com = await context.RechargeMethods.FirstOrDefaultAsync(x => x.Id == 2);
       com.ChangerAndCompanies.Add(new ChangerAndCompany
       {
           EnglishName = "eng",
           ArabicName = "arb"
       });
       com.ChangerAndCompanies.Add(new ChangerAndCompany
       {
           EnglishName = "popo",
           ArabicName = "baba"
       });
        await context.SaveChangesAsync();
        
    }
    public static async Task SeedChangers(DataContext context)
    {

      /*  var com = await context.PaymentMethods.FirstOrDefaultAsync(x => x.Id == 3);
        com.ChangerOffices.Add(new ChangerOffice(){EnglishName = "nname" , ArabicName = "arabic"});
        await context.SaveChangesAsync();*/
        
    }
}