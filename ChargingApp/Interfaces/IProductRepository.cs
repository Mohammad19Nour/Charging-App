using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface IProductRepository
{
    public Task<List<Product>> GetAllProductInCategory(int categoryId);
    public void AddProduct(Product product);
    public Task<bool> DeleteProductFromCategory(int productId);
    public Task<bool> SaveAllChangesAsync();
    public Task<Product?> GetProductByIdAsync(int productId);
    public IQueryable<Product> GetQuery();
}