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

    public void AddFavoriteProduct(Favorite fav)
    {
        _context.Favorites.Add(fav);
    }

    public void DeleteFavoriteProduct(Favorite fav)
    {
        _context.Favorites.Remove(fav);
    }

    public async Task<List<ProductDto>> GetFavoriteProductsForUserAsync(int userId)
    {
      var res = await _context.Favorites
            .Include(x=>x.User)
            .Include(x=>x.Product)
            .Include(x=>x.Product.AvailableQuantities)
            .Include(x=>x.Product.Photo)
            .Where(x=>x.UserId == userId)
          //  .ProjectTo<Product>(_mapper.ConfigurationProvider)
            .ToListAsync();

      return res.Select(t => _mapper.Map<ProductDto>(t.Product)).ToList();
    }

    public async Task<bool> CheckIfExist(int userId , int productId)
    {
        var res = await _context.Favorites
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId);
        return res != null;
    }
}