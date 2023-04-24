using ChargingApp.Entity;
using ChargingApp.Helpers;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChargingApp.Services;

public class BackgroundTask : IHostedService, IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;

    private Timer _timer;
    private Timer _timer2;
    private Timer _timer3;


    public BackgroundTask(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Check the orders status in other sites
        _timer = new Timer(CheckOrderFromOtherSites, null, TimeSpan.Zero,
            TimeSpan.FromMinutes(2));

        // Every month delete wrong, rejected and cancelled orders and payments
        _timer2 = new Timer(DeleteEveryMonth, null, TimeSpan.Zero,
            TimeSpan.FromDays(31));

        // Delete the notification history every 10 days
        _timer3 = new Timer(DeleteEveryTenDays, null, TimeSpan.Zero,
            TimeSpan.FromDays(10));

        return Task.CompletedTask;
    }

    private void DeleteEveryTenDays(object state)
    {
        Task.Run(async () =>
        {
            try
            {
                var dateTime = DateTime.UtcNow.AddDays(-10);
                using var scope = _scopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var notQuery = unitOfWork.NotificationRepository.GetQueryable();
                notQuery = notQuery.Where(x => x.CreatedAt < dateTime);
                var nots = await notQuery.ToArrayAsync();
                unitOfWork.NotificationRepository.DeleteNotificationHistory(nots);
                await unitOfWork.Complete();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        });
    }

    private void DeleteEveryMonth(object state)
    {
        Task.Run(async () =>
        {
            try
            {
                var dateTime = DateTime.UtcNow.AddDays(-31);
                using var scope = _scopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var orderQuery = unitOfWork.OrdersRepository.GetQueryable();
                var paymentQuery = unitOfWork.PaymentRepository.GetQueryable();

                orderQuery = orderQuery.Where(x => x.Status == 3  || x.Status == 5)
                    .Where(x => x.CreatedAt < dateTime);
                paymentQuery = paymentQuery.Where(x => x.Status == 3 || x.Status == 2)
                    .Where(x => x.CreatedDate < dateTime);

                var orders = await orderQuery.ToArrayAsync();
                var payments = await paymentQuery.ToArrayAsync();

                foreach (var order in orders)
                {
                    var notList = await unitOfWork.NotificationRepository
                        .GetOrdersNotifications(order.Id);
                    foreach (var not in notList)
                        unitOfWork.NotificationRepository.DeleteNotification(not);
                }

                foreach (var payment in payments)
                {
                    var notList = await unitOfWork.NotificationRepository
                        .GetOrdersNotifications(payment.Id);
                    foreach (var not in notList)
                        unitOfWork.NotificationRepository.DeleteNotification(not);
                }

                unitOfWork.OrdersRepository.DeleteOrders(orders);
                unitOfWork.PaymentRepository.DeletePayments(payments);

                await unitOfWork.Complete();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        });
    }

    private void CheckOrderFromOtherSites(object state)
    {
        Task.Run(async () =>
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var apiService = scope.ServiceProvider.GetRequiredService<IApiService>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                var orders = await unitOfWork
                    .OtherApiRepository.GetAllOrdersAsync();

                foreach (var t in orders)
                {
                    var res = await apiService.CheckOrderStatusAsync(t.ApiOrderId,
                        t.HostingSite.BaseUrl, t.HostingSite.Token);
                    if (!res.Succeed) continue;

                    if (res.Status.ToLower() != "accept" && res.Status.ToLower() != "reject") continue;

                    var ourOrder = await unitOfWork.OrdersRepository.GetOrderByIdAsync(t.Order.Id);
                    if (ourOrder == null) continue;

                    ourOrder.Status = res.Status.ToLower() switch
                    {
                        "accept" => 1,
                        "reject" => 2,
                        _ => ourOrder.Status
                    };

                    ourOrder.Notes = res.Status;
                    var roles = await userManager.GetRolesAsync(ourOrder.User);

                    var lastLevel = ourOrder.User.VIPLevel;
                    if (ourOrder.Status == 2 && roles.Any(x => x.ToLower() == "vip"))
                    {
                        ourOrder.User.Balance += ourOrder.TotalPrice;
                        ourOrder.User.TotalPurchasing -= ourOrder.TotalPrice;
                        ourOrder.User.TotalForVIPLevel -= ourOrder.TotalPrice;
                        ourOrder.User.VIPLevel = await unitOfWork.VipLevelRepository
                            .GetVipLevelForPurchasingAsync(ourOrder.User.TotalForVIPLevel);

                        if (lastLevel > ourOrder.User.VIPLevel)
                        {
                            var curr = new NotificationHistory
                            {
                                User = ourOrder.User,
                                ArabicDetails = " تم اعادة مستواك الى vip  " + ourOrder.User.VIPLevel,
                                EnglishDetails = "Your level has been returned back to vip " + ourOrder.User.VIPLevel
                            };
                            unitOfWork.NotificationRepository.AddNotificationForHistoryAsync(curr);
                        }
                    }

                    var not = new OrderAndPaymentNotification
                    {
                        Order = ourOrder,
                        User = ourOrder.User
                    };

                    unitOfWork.OtherApiRepository.DeleteOrder(ourOrder.Id);
                    await notificationService.NotifyUserByEmail(ourOrder.User.Email, unitOfWork, not,
                        "Order status notification", SomeUsefulFunction.GetOrderNotificationDetails(ourOrder));
                }

                await unitOfWork.Complete();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        });
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        _timer2?.Change(Timeout.Infinite, 0);
        _timer3?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer.Dispose();
        _timer2.Dispose();
        _timer3.Dispose();
    }
}