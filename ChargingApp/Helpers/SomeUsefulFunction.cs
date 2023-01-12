using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Interfaces;

namespace ChargingApp.Helpers;

public static class SomeUsefulFunction
{
    public static async Task<double> CalcTotalPrice(int dtoQuantity, Product product,
        AppUser user, IUnitOfWork unitOfWork)
    {
        var specificPrice = await unitOfWork.SpecificPriceForUserRepository
            .GetProductPriceForUserAsync(product.Id, user);
        var priceX = product.Price;

        if (specificPrice != null)
        {
            priceX = (double)specificPrice * dtoQuantity;

            user.TotalPurchasing += priceX;
            user.TotalForVIPLevel += priceX;
            user.VIPLevel = await unitOfWork.VipLevelRepository
                .GetVipLevelForPurchasingAsync(user.TotalForVIPLevel);

            return priceX;
        }

        var vipLevels = await unitOfWork.VipLevelRepository.GetAllVipLevelsAsync();
        var benefitPercent = new List<int>();
        var minimumP = new List<int>();

        foreach (var x in vipLevels)
        {
            benefitPercent.Add(x.BenefitPercent);
            minimumP.Add(x.MinimumPurchase);
        }

        var total = dtoQuantity * priceX;

        double price = 0;

        minimumP.Add(1000000000);

        for (var i = 0; i < benefitPercent.Count; i++)
        {
            if (total == 0) break;

            if (minimumP[i] < user.TotalForVIPLevel && minimumP[i + 1] < user.TotalForVIPLevel) continue;

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
            price += d;
            user.VIPLevel = vipLevels[i].VIP_Level;
            if (total == 0) break;

            if (specificBenefit is null)
                total -= total * globalBenefit / 100;
            else total -= total * (double)specificBenefit / 100;
        }

        return price;
    }

    public static async Task<List<ProductDto>> CalcPriceForProducts(AppUser? user, List<ProductDto> products,
        IUnitOfWork _unitOfWork, int vipLevel)
    {
        var tmp = products;

        var benefitPercent = await _unitOfWork.VipLevelRepository.GetBenefitPercentForVipLevel(vipLevel);

        var syria = await _unitOfWork.CurrencyRepository.GetSyrianCurrency();
        var turkey = await _unitOfWork.CurrencyRepository.GetSyrianCurrency();

        foreach (var t in products)
        {
            var specificBenefitPercent = await
                _unitOfWork.BenefitPercentInSpecificVipLevelRepository
                    .GetBenefitPercentForProductAsync(t.Id, vipLevel);

            if (specificBenefitPercent is null)
                t.Price += t.Price * benefitPercent / 100;
            else t.Price += t.Price * (double)specificBenefitPercent / 100;

            t.TurkishPrice = t.Price * turkey;
            t.SyrianPrice = t.Price * syria;
        }

        if (user is null || vipLevel == 0) return products;
        Console.WriteLine(vipLevel+"\n\n");


        foreach (var t in products)
        {
            var specificPrice = await
                _unitOfWork.SpecificPriceForUserRepository
                    .GetProductPriceForUserAsync(t.Id, user);

            if (specificPrice == null) continue;
            
            t.Price = (double)specificPrice ;
            t.TurkishPrice = t.Price * turkey;
            t.SyrianPrice = t.Price * syria;
        }

        return products;
    }
}