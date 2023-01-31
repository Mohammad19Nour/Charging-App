using ChargingApp.DTOs;
using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface IProductRepository
{
    public Task<List<Product>> GetAllProductInCategory(int categoryId);
    public void AddProduct(Product product);
    public void DeleteProductFromCategory(Product product);
    public Task<Product?> GetProductByIdAsync(int productId);
    public IQueryable<Product> GetQuery();
    public void UpdateProduct(Product product);
}