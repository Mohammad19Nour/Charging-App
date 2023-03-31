using ChargingApp.Services;

namespace ChargingApp.Interfaces;

public interface IApiService
{
    public Task<bool> CheckProductByIdIfExistAsync(int id, string baseUrl, string token);

    public Task<(bool Success, string Message, int OrderId)> SendOrderAsync(int productId, decimal qty, string playerId,
        string baseUrl, string token);

    public Task<(bool Succeed, string Status )> CheckOrderStatusAsync(int orderId, string baseUrl, string token);
    public Task<(bool Status,string Message  , List<ApiService.ProductResponse>? Products)> GetAllProductsAsync(string baseUrl, string token);
    public Task<(bool Success, string Message)> CancelOrderByIdAsync(int apiOrderId, string baseUrl, string token);
}