using ChargingApp.Entity;

namespace ChargingApp.Helpers;

public static class SomeUsefulFunction
{
    public static double CalcTotalePrice(int dtoQuantity, double productPrice , AppUser user , List<VIPLevels> vipLevels)
    {
        double total = dtoQuantity * productPrice;
        
        List<int> discounts = new List<int>();
        List<int> minimumP = new List<int>();
        foreach (var x in vipLevels)
        {
            discounts.Add(x.Discount);
            minimumP.Add(x.MinimumPurchase);
        }

        double price = 0;
        
        minimumP.Add(1000000000);

        for (int i = 0; i < discounts.Count; i++)
        {
            if (total == 0) break;
            
            if (minimumP[i] < user.TotalPurchasing && minimumP[i+1] < user.TotalPurchasing) continue;

            total = total - total * discounts[i] / 100;
            
            double d = minimumP[i + 1] - user.TotalPurchasing;

            d = Math.Min(d , total);

            total -= d ;
            
            user.TotalPurchasing += d ;
            price += d ;
            user.VIPLevel = vipLevels[i].VIP_Level;
            if(total == 0) break;

            total = total + total * discounts[i] / 100;
        }
        return price;
    }
}