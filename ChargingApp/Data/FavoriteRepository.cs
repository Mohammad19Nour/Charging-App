using AutoMapper;
using AutoMapper.QueryableExtensions;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChargingApp.Data;

public class FavoriteRepository : IFavoriteRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public FavoriteRepository(DataContext context , IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public void AddFavoriteCategory(Favorite fav)
    {
        _context.Favorites.Add(fav);
    }

    public void DeleteFavoriteCategory(Favorite fav)
    {
        _context.Favorites.Remove(fav);
    }

    public async Task<List<CategoryWithProductsDto>> GetFavoriteCategoriesForUserAsync(int userId)
    {
        var res = _context.Favorites
            .Include(x => x.User)
            .Include(x => x.Category)
            .Include(x => x.Category.Photo)
            .Include(x=>x.Category.Products)
            .Where(x => x.UserId == userId);

      return await (_mapper.ProjectTo<CategoryWithProductsDto>(res.Select(t=>t.Category))).ToListAsync();
    }

    public async Task<bool> CheckIfExist(int userId , int categoryId)
    {
        var res = await _context.Favorites
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId && x.CategoryId == categoryId);
        return res != null;
    }
}