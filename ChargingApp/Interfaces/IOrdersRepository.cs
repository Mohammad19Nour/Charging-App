using ChargingApp.DTOs;
using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface IOrdersRepository
{
    public void AddOrder(Order order);
    public void DeleteOrders(Order[] orders);
    public void DeleteOrder(Order order);
    public Task<Order?> GetOrderByIdAsync(int orderId);
    public Task<List<NormalOrderDto>> GetNormalUserOrdersAsync(int userId);
    public Task<List<OrderDto>> GetVipUserOrdersAsync(int userId);
    public Task<List<Order>> GetPendingOrdersAsync(string email = "");
    public Task<bool> CheckPendingOrdersForUserByEmailAsync(string email);
    public Task<List<Order>> GetDoneOrders( DateQueryDto dto,string? user );
    public Task<List<Order?>> GetOrdersForSpecificProduct(int productId);
    public IQueryable<Order> GetQueryable();
}