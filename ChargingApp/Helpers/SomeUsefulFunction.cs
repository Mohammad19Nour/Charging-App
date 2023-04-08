using System.Text.RegularExpressions;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Interfaces;

namespace ChargingApp.Helpers;

public static class SomeUsefulFunction
{
    private static readonly List<string> Status = new List<string>
        { "Pending", "Succeed", "Rejected", "Wrong", "Received", "Cancelled" };

    public static Dictionary<string, dynamic> GetVipLevelNotification(int lvl)
    {
        return new Dictionary<string, dynamic>
        {
            { "status", "Your vip level is : " + lvl }
        };
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

    public static bool CheckIfItIsAnAdmin(IList<string> roles)
    {
        return roles.Any(x =>
        {
            var tmp = x.ToLower();
            return tmp is "admin" or "admin_1" or "admin_2" or "advancedemployee" or "normalemployee";
        });
    }
}