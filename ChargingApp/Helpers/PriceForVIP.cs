using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Interfaces;

namespace ChargingApp.Helpers;

public static class PriceForVIP
{
    public static async Task<decimal> CannotChooseQuantity
    (decimal dtoQuantity, Product product,
        AppUser user, IUnitOfWork unitOfWork)
    {
        var specificPrice = await unitOfWork.SpecificPriceForUserRepository
            .GetProductPriceForUserAsync(product.Id, user);

        var priceX = product.Price;

        if (specificPrice != null)
        {
            priceX = (decimal)specificPrice * dtoQuantity;

            user.TotalPurchasing += priceX;
            user.TotalForVIPLevel += priceX;

            user.VIPLevel = await unitOfWork.VipLevelRepository
                .GetVipLevelForPurchasingAsync(user.TotalForVIPLevel);
            return priceX;
        }

        var vipLevels = await unitOfWork.VipLevelRepository.GetAllVipLevelsAsync();

        var total = dtoQuantity * priceX;

        var benefitPercent = new List<decimal>();
        var minimumP = new List<decimal>();

        vipLevels = vipLevels.Where(x => x.VipLevel != 0).ToList();

        foreach (var x in vipLevels)
        {
            benefitPercent.Add(x.BenefitPercent);
            minimumP.Add(x.MinimumPurchase);
        }

        decimal price = 0;

        minimumP.Add(5000000000);
        var specificBenefit = await unitOfWork.SpecificBenefitPercentRepository
            .GetBenefitPercentForProductAsync(product.Id);

        for (var i = 0; i < benefitPercent.Count; i++)
        {
            if (minimumP[i] < user.TotalForVIPLevel && minimumP[i + 1] < user.TotalForVIPLevel) continue;

            user.VIPLevel = await unitOfWork.VipLevelRepository
                .GetVipLevelForPurchasingAsync(user.TotalForVIPLevel);

            var globalBenefit = benefitPercent[i];

            if (specificBenefit is null)
                total += total * globalBenefit / 100;
            else total += total * (decimal)specificBenefit / 100;

            var d = minimumP[i + 1] - user.TotalForVIPLevel;

            d = Math.Min(d, total);

            total -= d;

            user.TotalPurchasing += d;
            user.TotalForVIPLevel += d;
            user.VIPLevel = vipLevels[i].VipLevel;
            price += d;

            if (total == 0) break;

            if (specificBenefit is null)
                total /= (globalBenefit / 100 + 1);
            else total /= (decimal)(specificBenefit / 100 + 1);
        }

        return price;
    }


    public static async Task<decimal> CalcTotalQuantity(decimal dtoQuantity, Product product,
        AppUser user, IUnitOfWork unitOfWork)
    {
        var specificBenefit = await unitOfWork.SpecificBenefitPercentRepository
            .GetBenefitPercentForProductAsync(product.Id);

        if (specificBenefit != null) return dtoQuantity - dtoQuantity * (decimal)specificBenefit / 100;

        var benefit = await
            unitOfWork.VipLevelRepository.GetBenefitPercentForVipLevel(user.VIPLevel);

        return dtoQuantity - dtoQuantity * benefit / 100;
    }

    public static async Task<List<ProductDto>> CalcPriceForProducts(AppUser user, List<ProductDto> products,
        IUnitOfWork unitOfWork, int vipLevel)
    {
        var syria = await unitOfWork.CurrencyRepository.GetSyrianCurrency();
        var turkey = await unitOfWork.CurrencyRepository.GetTurkishCurrency();

        var benefitPercent =
            await unitOfWork.VipLevelRepository.GetBenefitPercentForVipLevel(vipLevel);

        foreach (var t in products)
        {
            var specificPrice = await unitOfWork.SpecificPriceForUserRepository
                .GetProductPriceForUserAsync(t.Id, user);
            
            var specificBenefitPercent = await
                unitOfWork.SpecificBenefitPercentRepository
                    .GetBenefitPercentForProductAsync(t.Id);
            
            
            if (!t.CanChooseQuantity)
            {
                if (specificPrice != null)
                {
                    t.Price = (decimal)specificPrice;
                }
                else
                {
                    if (specificBenefitPercent is null)
                        t.Price += t.Price * benefitPercent / 100;
                    else t.Price += t.Price * (decimal)specificBenefitPercent / 100;
                }
            }
            else
            {
                 specificPrice = await unitOfWork.SpecificPriceForUserRepository
                    .GetProductPriceForUserAsync(t.Id, user);

                if (specificPrice != null)
                {
                    t.Price = (decimal)specificPrice;
                }
                else
                {
                    if (specificBenefitPercent is null)
                        t.Quantity -= t.Quantity * benefitPercent / 100;
                    else t.Quantity -= t.Quantity * (decimal)specificBenefitPercent / 100;
                }
            }

            t.TurkishPrice = t.Price * turkey;
            t.SyrianPrice = t.Price * syria;
        }

        return products;
    }
}