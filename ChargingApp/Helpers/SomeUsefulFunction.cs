using System.Text.RegularExpressions;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Interfaces;

namespace ChargingApp.Helpers;

public static class SomeUsefulFunction
{
    private static readonly List<string> Status = new List<string> { "Pending", "Succeed", "Rejected", "Wrong", "Received", "Cancelled" };
    public static async Task<double> CalcTotalPriceCannotChooseQuantity
    (double dtoQuantity, Product product,
        AppUser user, IUnitOfWork unitOfWork)
    {
        var specificPrice = await unitOfWork.SpecificPriceForUserRepository
            .GetProductPriceForUserAsync(product.Id, user);

        var priceX = product.Price;

        if (specificPrice != null)
        {
            priceX = (double)specificPrice * dtoQuantity;

            if (user.VIPLevel == 0) return priceX;

            user.TotalPurchasing += priceX;

            user.TotalForVIPLevel += priceX;
            user.VIPLevel = await unitOfWork.VipLevelRepository
                .GetVipLevelForPurchasingAsync(user.TotalForVIPLevel);

            return priceX;
        }
        
        var vipLevels = await unitOfWork.VipLevelRepository.GetAllVipLevelsAsync();

        var total = dtoQuantity * priceX;

        if (user.VIPLevel == 0)
        {
            total += total * vipLevels[0].BenefitPercent / 100;
            return total;
        }

        var benefitPercent = new List<double>();
        var minimumP = new List<double>();

        vipLevels = vipLevels.Where(x => x.VipLevel != 0).ToList();

        foreach (var x in vipLevels)
        {
            benefitPercent.Add(x.BenefitPercent);
            minimumP.Add(x.MinimumPurchase);
        }

        double price = 0;

        minimumP.Add(1000000000);

        for (var i = 0; i < benefitPercent.Count; i++)
        {
            if (minimumP[i] < user.TotalForVIPLevel && minimumP[i + 1] < user.TotalForVIPLevel) continue;


            user.VIPLevel = await unitOfWork.VipLevelRepository
                .GetVipLevelForPurchasingAsync(user.TotalForVIPLevel);
            var specificBenefit = await unitOfWork.BenefitPercentInSpecificVipLevelRepository
                .GetBenefitPercentForProductAsync(product.Id, user.VIPLevel);
            var globalBenefit = benefitPercent[i];
            

            if (specificBenefit is null)
                total += total * globalBenefit / 100;
            else total += total * (double)specificBenefit / 100;

            var d = minimumP[i + 1] - user.TotalForVIPLevel;

            d = Math.Min(d, total);

            total -= d;

            user.TotalPurchasing += d;
            user.TotalForVIPLevel += d;
            user.VIPLevel = vipLevels[i].VipLevel;
            price += d;
            
             if (total == 0) break;

            if (specificBenefit is null)
                total -= total * globalBenefit / 100;
            else total -= total * (double)specificBenefit / 100;
        }

        return price;
    }

    public static async Task<double> CalcTotalQuantity(double dtoQuantity, Product product,
        AppUser user, IUnitOfWork unitOfWork)
    {
        var vipLevels = await unitOfWork.VipLevelRepository.GetAllVipLevelsAsync();

        var total = dtoQuantity;

        if (user.VIPLevel == 0)
        {
            total -= total * vipLevels[0].BenefitPercent / 100;
            return total;
        }

        var benefit = await
            unitOfWork.VipLevelRepository.GetBenefitPercentForVipLevel(user.VIPLevel);

        return total - total * benefit / 100;
    }

    public static async Task<List<ProductDto>> CalcPriceForProducts(AppUser? user, List<ProductDto> products,
        IUnitOfWork unitOfWork, int vipLevel)
    {
        var benefitPercent =
            await unitOfWork.VipLevelRepository.GetBenefitPercentForVipLevel(vipLevel);

        var syria = await unitOfWork.CurrencyRepository.GetSyrianCurrency();
        var turkey = await unitOfWork.CurrencyRepository.GetSyrianCurrency();

        foreach (var t in products)
        {
            var specificBenefitPercent = await
                unitOfWork.BenefitPercentInSpecificVipLevelRepository
                    .GetBenefitPercentForProductAsync(t.Id, vipLevel);
            if (!t.CanChooseQuantity)
            {
                if (specificBenefitPercent is null)
                    t.Price += t.Price * benefitPercent / 100;
                else t.Price += t.Price * (double)specificBenefitPercent / 100;
            }
            else
            {
                if (specificBenefitPercent is null)
                    t.Quantity -= t.Quantity * benefitPercent / 100;
                else t.Quantity -= t.Quantity * (double)specificBenefitPercent / 100;
            }

            t.TurkishPrice = t.Price * turkey;
            t.SyrianPrice = t.Price * syria;
        }

        if (user is null || vipLevel == 0) return products;

        foreach (var t in products)
        {
            if (t.CanChooseQuantity) continue;

            var specificPrice = await
                unitOfWork.SpecificPriceForUserRepository
                    .GetProductPriceForUserAsync(t.Id, user);

            if (specificPrice == null) continue;

            t.Price = (double)specificPrice;
            t.TurkishPrice = t.Price * turkey;
            t.SyrianPrice = t.Price * syria;
        }

        return products;
    }

    public static async Task SendVipLevelNotification()
    {
        
    }

   public static Dictionary<string, dynamic> GetPaymentNotificationDetails(Payment order)
    {
        return new Dictionary<string, dynamic>
        {
            { "paymentId", order.Id },
            { "status", "payment " + Status[order.Status] }
        };
    }
   
   
   public static Dictionary<string, dynamic> GetOrderNotificationDetails(Order order)
   {
       return new Dictionary<string, dynamic>
       {
           { "orderId", order.Id },
           { "status", "order " + Status[order.Status] },
       };
   }
   public static bool IsValidEmail(string email)
   {
       const string pattern = @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*" + "@" + @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$";
       return Regex.IsMatch(email, pattern);
   }
}