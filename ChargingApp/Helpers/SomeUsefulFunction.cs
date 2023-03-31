using System.Text.RegularExpressions;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Interfaces;

namespace ChargingApp.Helpers;

public static class SomeUsefulFunction
{
    private static readonly List<string> Status = new List<string>
        { "Pending", "Succeed", "Rejected", "Wrong", "Received", "Cancelled" };

    public static async Task<decimal> CalcTotalPriceCannotChooseQuantity
    (decimal dtoQuantity, Product product,
        AppUser user, IUnitOfWork unitOfWork)
    {
        var specificPrice = await unitOfWork.SpecificPriceForUserRepository
            .GetProductPriceForUserAsync(product.Id, user);

        var priceX = product.Price;

        if (specificPrice != null)
        {
            priceX = (decimal)specificPrice * dtoQuantity;

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

        var benefitPercent = new List<decimal>();
        var minimumP = new List<decimal>();

        vipLevels = vipLevels.Where(x => x.VipLevel != 0).ToList();

        foreach (var x in vipLevels)
        {
            benefitPercent.Add(x.BenefitPercent);
            minimumP.Add(x.MinimumPurchase);
        }

        decimal price = 0;

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
        var vipLevels = await unitOfWork.VipLevelRepository.GetAllVipLevelsAsync();

        var total = dtoQuantity;

        var specificBenefit = await unitOfWork.BenefitPercentInSpecificVipLevelRepository
            .GetBenefitPercentForProductAsync(product.Id, user.VIPLevel);

        if (specificBenefit != null) return total - total * (decimal)specificBenefit / 100;

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
        var syria = await unitOfWork.CurrencyRepository.GetSyrianCurrency();
        var turkey = await unitOfWork.CurrencyRepository.GetSyrianCurrency();
        if (vipLevel != 0 && user != null)
        {
            var benefitPercent =
                await unitOfWork.VipLevelRepository.GetBenefitPercentForVipLevel(vipLevel);

            foreach (var t in products)
            {
                var specificBenefitPercent = await
                    unitOfWork.BenefitPercentInSpecificVipLevelRepository
                        .GetBenefitPercentForProductAsync(t.Id, vipLevel);
                if (!t.CanChooseQuantity)
                {
                    var specificPrice = await unitOfWork.SpecificPriceForUserRepository
                        .GetProductPriceForUserAsync(t.Id, user);

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
                    var specificPrice = await unitOfWork.SpecificPriceForUserRepository
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
        else // normal user or no auth
        {
            var benefitPercent =
                await unitOfWork.VipLevelRepository.GetBenefitPercentForVipLevel(0);

            foreach (var t in products)
            {
                var specificBenefitPercent = await
                    unitOfWork.BenefitPercentInSpecificVipLevelRepository
                        .GetBenefitPercentForProductAsync(t.Id, vipLevel);

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
        const string pattern = @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*" + "@" +
                               @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$";
        return Regex.IsMatch(email, pattern);
    }

    public static (bool Res, string Message) CheckDate(DateQueryDto dto)
    {
        if (dto.Year == null)
            return (false, "you should specify year");

        if (dto.Month is null && dto.Day != null)
            return (false, "you should specify month of day");

        return (true, "");
    }
}