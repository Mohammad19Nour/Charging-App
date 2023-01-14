using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChargingApp.Data;

public class ProductRepository : IProductRepository
{
    private readonly DataContext _context;

    public ProductRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetAllProductInCategory(int categoryId)
    {
        return await _context.Products
            .Include(c=>c.Category)
            .Include(x=>x.Photo)
            .Where(x => x.CategoryId == categoryId)
            .ToListAsync();
    }

    public void AddProduct(Product product)
    {
        _context.Products.Add(product);
    }

    public void DeleteProductFromCategory(Product product)
    {
        _context.Photos.Remove(product.Photo);
        _context.Products.Remove(product);
    }

    public async Task<Product?> GetProductByIdAsync(int productId)
    {
        return await _context.Products
            .Include(c=>c.Category)
            .Include(x => x.AvailableQuantities)
            .Include(p=>p.Photo)
            .FirstOrDefaultAsync(x =>  productId == x.Id);
    }


    public IQueryable<Product> GetQuery()
    {
        return _context.Products.AsQueryable();
    }

}