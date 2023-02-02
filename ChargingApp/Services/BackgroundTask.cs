using ChargingApp.Interfaces;

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
            TimeSpan.FromSeconds(100000));

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

                var orders = await unitOfWork
                    .OtherApiRepository.GetAllOrdersAsync();

                foreach (var t in orders)
                {
                    var res = await apiService.CheckOrderStatusAsync(t.ApiOrderId);
                    if (!res.Succeed) continue;

                    if (res.Status.ToLower() != "accept" && res.Status.ToLower() != "reject") continue;
                 
                    var ourOrder = await unitOfWork.OrdersRepository.GetOrderByIdAsync(t.Order.Id);
                    ourOrder.Status = res.Status.ToLower() switch
                    {
                        "accept" => 1,
                        "reject" => 2,
                        _ => ourOrder.Status
                    };
                    unitOfWork.OtherApiRepository.DeleteOrder(t.ApiOrderId);
                      Console.WriteLine(res.Status+"\n\n\n");
                }

                Console.WriteLine(orders.Count + "\n***\n");
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
        _timer?.Dispose();
    }
}