using ChargingApp.DTOs;
using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface IFavoriteRepository
{
    public void AddFavoriteProduct(Favorite fav);
    public void DeleteFavoriteProduct(Favorite fav);
    public Task<List<ProductDto>> GetFavoriteProductsForUserAsync(int userId);
    public Task<bool> CheckIfExist(int userId , int productId);

}