using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface IOtherApiRepository
{
    public Task<bool> CheckIfProductExistAsync(int productId, bool ourProduct);
    public Task<bool> CheckIfOrderExistAsync(int orderId, bool ourOrder);
    public void AddProduct(ApiProduct product);
    public void AddOrder(ApiOrder order);
    public void DeleteProduct(int productId);
    public void DeleteOrder(int orderId);
    public Task<int> GetApiProductIdAsync(int ourProductId);
    public Task<int> GetApiOrderIdAsync(int ourOrderId);
    public Task<List<ApiOrder>> GetAllOrdersAsync();
    public Task<List<ApiProduct>> GetAllProductsAsync();
}