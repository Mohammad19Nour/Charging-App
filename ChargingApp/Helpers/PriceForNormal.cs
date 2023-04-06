using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Interfaces;

namespace ChargingApp.Helpers;

public static class PriceForNormal
{
    public static async Task<decimal> CannotChooseQuantity
    (decimal dtoQuantity, Product product,
        AppUser user, IUnitOfWork unitOfWork)
    {
        var priceX = product.Price;

        var vipLevels = await unitOfWork.VipLevelRepository.GetAllVipLevelsAsync();

        var total = dtoQuantity * priceX;

        var specificBenefit = await unitOfWork.SpecificBenefitPercentRepository
            .GetBenefitPercentForProductAsync(product.Id);

        if (specificBenefit != null)
            total += total * specificBenefit.Value / 100;

        else
            total += total * vipLevels[0].BenefitPercent / 100;

        return total;
    }

    public static async Task<decimal> CalcTotalQuantity(decimal dtoQuantity, Product product,
        AppUser user, IUnitOfWork unitOfWork)
    {
        var vipLevels = await unitOfWork.VipLevelRepository.GetAllVipLevelsAsync();

        var total = dtoQuantity;

        var specificBenefit = await unitOfWork.SpecificBenefitPercentRepository
            .GetBenefitPercentForProductAsync(product.Id);

        if (specificBenefit != null) total -= -total * (decimal)specificBenefit / 100;
        else
            total -= total * vipLevels[0].BenefitPercent / 100;
        return total;
    }

    public static async Task<List<ProductDto>> CalcPriceForProducts
    (AppUser? user, List<ProductDto> products,
        IUnitOfWork unitOfWork, int vipLevel)
    {
        var syria = await unitOfWork.CurrencyRepository.GetSyrianCurrency();
        var turkey = await unitOfWork.CurrencyRepository.GetSyrianCurrency();

        var benefitPercent =
            await unitOfWork.VipLevelRepository.GetBenefitPercentForVipLevel(0);

        foreach (var t in products)
        {
            var specificBenefitPercent = await
                unitOfWork.SpecificBenefitPercentRepository
                    .GetBenefitPercentForProductAsync(t.Id);

            if (!t.CanChooseQuantity)
            {
                if (specificBenefitPercent is null)
                    t.Price += t.Price * benefitPercent / 100;
                else t.Price += t.Price * (decimal)specificBenefitPercent / 100;
            }
            else
            {
                if (specificBenefitPercent is null)
                    t.Quantity -= t.Quantity * benefitPercent / 100;
                else t.Quantity -= t.Quantity * (decimal)specificBenefitPercent / 100;
            }

            t.TurkishPrice = t.Price * turkey;
            t.SyrianPrice = t.Price * syria;
        }

        return products;
    }
}