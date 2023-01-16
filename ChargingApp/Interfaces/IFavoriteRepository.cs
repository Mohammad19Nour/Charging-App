using ChargingApp.DTOs;
using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface IFavoriteRepository
{
    public void AddFavoriteCategory(Favorite fav);
    public void DeleteFavoriteCategory(Favorite fav);
    public Task<List<CategoryDto>> GetFavoriteCategoriesForUserAsync(int userId);
    public Task<bool> CheckIfExist(int userId , int categoryId);

}