using ChargingApp.Entity;
using ChargingApp.Helpers;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace ChargingApp.Services;

public class BackgroundTask : IHostedService, IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;

    private Timer _timer;

    public BackgroundTask(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(DoWork, null, TimeSpan.Zero,
            TimeSpan.FromMinutes(1));

        return Task.CompletedTask;
    }

    private void DoWork(object state)
    {
        Task.Run(async () =>
        {
            using (var scope = _scopeFactory.CreateScope())
            {
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

                    if (ourOrder.Status == 2 && roles.Any(x => x.ToLower() == "vip"))
                    {
                        ourOrder.User.Balance += ourOrder.TotalPrice;
                        ourOrder.User.TotalPurchasing -= ourOrder.TotalPrice;
                        ourOrder.User.TotalForVIPLevel -= ourOrder.TotalPrice;
                        ourOrder.User.VIPLevel = await unitOfWork.VipLevelRepository
                            .GetVipLevelForPurchasingAsync(ourOrder.User.TotalForVIPLevel);
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
        });
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer.Dispose();
    }
}