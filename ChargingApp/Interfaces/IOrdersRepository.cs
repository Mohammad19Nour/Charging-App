using ChargingApp.DTOs;
using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface IOrdersRepository
{
    public void AddOrder(Order order);
    public void DeleteOrder(Order order);
    public Task<Order?> GetOrderByIdAsync(int orderId);
    public Task<List<NormalOrderDto>> GetNormalUserOrdersAsync(int userId);
    public Task<List<OrderDto>> GetVipUserOrdersAsync(int userId);
    public Task<Order?> GetLastOrderForUserByIdAsync(int userId);
    public Task<List<PendingOrderDto>> GetPendingOrdersAsync(string email = "");
    public Task<bool> CheckPendingOrdersForUserByEmailAsync(string email);
}