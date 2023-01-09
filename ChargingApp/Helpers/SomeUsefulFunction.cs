using ChargingApp.Entity;

namespace ChargingApp.Helpers;

public static class SomeUsefulFunction
{
    public static double CalcTotalPrice(int dtoQuantity, double productPrice , AppUser user , List<VIPLevels> vipLevels)
    {
        var total = dtoQuantity * productPrice;
        
        var discounts = new List<int>();
        var minimumP = new List<int>();
        foreach (var x in vipLevels)
        {
            discounts.Add(x.Discount);
            minimumP.Add(x.MinimumPurchase);
        }

        double price = 0;
        
        minimumP.Add(1000000000);

        for (var i = 0; i < discounts.Count; i++)
        {
            if (total == 0) break;
            
            if (minimumP[i] < user.TotalForVIPLevel && minimumP[i+1] < user.TotalForVIPLevel) continue;

            total -= total * discounts[i] / 100;
            
            var d = minimumP[i + 1] - user.TotalForVIPLevel;

            d = Math.Min(d , total);

            total -= d ;
            
            user.TotalPurchasing += d ;
            user.TotalForVIPLevel += d;
            price += d ;
            user.VIPLevel = vipLevels[i].VIP_Level;
            if(total == 0) break;

            total += total * discounts[i] / 100;
        }
        return price;
    }
}