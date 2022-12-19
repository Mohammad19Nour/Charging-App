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
            .Where(x => x.CategoryId == categoryId)
            .ToListAsync();
    }

    public void AddProduct(Product product)
    {
        _context.Products.Add(product);
    }

    public async Task<bool> DeleteProductFromCategory(int productId)
    {
        var product = await GetProductByIdAsync(productId);

        _context.Products.Remove(product);
        return await SaveAllChangesAsync();
    }

    public async Task<bool> SaveAllChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<Product?> GetProductByIdAsync(int productId)
    {
        return await _context.Products
            .Include(c=>c.Category)
            .Include(x => x.AvailableQuantities)
            .FirstOrDefaultAsync(x =>  productId == x.Id);
    }


    public IQueryable<Product> GetQuery()
    {
        return _context.Products.AsQueryable();
    }
}