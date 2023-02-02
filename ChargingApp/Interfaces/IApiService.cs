using ChargingApp.Services;

namespace ChargingApp.Interfaces;

public interface IApiService
{
    public Task<bool> CheckProductByIdIfExistAsync(int id);
    public Task<(bool Success , string Message , int OrderId)> SendOrderAsync(int productId , double qty , string playerId);
    public Task<(bool Succeed , string Status )> CheckOrderStatusAsync(int orderId);
    public Task<List<ApiService.ProductResponse>?> GetAllProductsAsync();
}