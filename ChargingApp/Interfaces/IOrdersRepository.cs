using ChargingApp.DTOs;
using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface IOrdersRepository
{
    public void AddOrder(Order order);
    public Task<bool> SaveAllChangesAsync();
    public Task<bool> DeleteOrderByIdAsync(int orderId);
    public void DeleteOrderById(int orderId);

    public Task<Order?> GetOrderByIdAsync(int orderId);
    public Task<List<NormalOrderDto>> GetNormalUserOrdersAsync(int userId);
    public Task<List<OrderDto>> GetVipUserOrdersAsync(int userId);

    public Task<Order?> GetLastOrderForUserByIdAsync(int userId);
}