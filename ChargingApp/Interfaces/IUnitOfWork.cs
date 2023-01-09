namespace ChargingApp.Interfaces;

public interface IUnitOfWork
{
    ICategoryRepository CategoryRepository { get; }
    IUserRepository UserRepository { get; }
    IOrdersRepository OrdersRepository { get; }
    IPaymentRepository PaymentRepository { get; }
    IPaymentGatewayRepository PaymentGatewayRepository { get; }
    IProductRepository ProductRepository { get; }
    IRechargeCodeRepository RechargeCodeRepository { get; }
    IRechargeMethodeRepository RechargeMethodeRepository { get; }
    IVipLevelRepository VipLevelRepository { get; }
    ICurrencyRepository CurrencyRepository { get; }
    IOurAgentsRepository OurAgentsRepository { get; }

    Task<bool> Complete();
    bool HasChanges();

}