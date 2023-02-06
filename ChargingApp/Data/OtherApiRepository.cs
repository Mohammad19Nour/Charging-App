using ChargingApp.Entity;
using ChargingApp.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChargingApp.Data;

public class OtherApiRepository : IOtherApiRepository
{
    private readonly DataContext _context;

    public OtherApiRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<bool> CheckIfProductExistAsync(int productId, bool ourProduct)
    {
        var query = _context.ApiProducts
            .Include(x => x.Product)
            .AsNoTracking();

        query = ourProduct
            ? query.Where(x => x.Product.Id == productId)
            : query.Where(x => x.ApiProductId == productId);

        return await query.FirstOrDefaultAsync() != null;
    }

    public async Task<bool> CheckIfOrderExistAsync(int orderId, bool ourOrder)
    {
        var query = _context.ApiOrders
            .Include(x => x.Order)
            .AsNoTracking();

        query = ourOrder
            ? query.Where(x => x.Order.Id == orderId)
            : query.Where(x => x.ApiOrderId == orderId);

        return await query.FirstOrDefaultAsync() != null;
    }

    public void AddProduct(ApiProduct product)
    {
        _context.ApiProducts.Add(product);
    }

    public void AddOrder(ApiOrder order)
    {
        _context.ApiOrders.Add(order);
    }

    public void DeleteProduct(int productId)
    {
        var product = _context.ApiProducts
            .Include(x=>x.Product)
            .First(x => x.Product.Id == productId);
        _context.ApiProducts.Remove(product);
    }

    public void DeleteOrder(int orderId)
    {
        var order = _context.ApiOrders
            .Include(x=>x.Order)
            .First(x => x.Order.Id == orderId);
        _context.ApiOrders.Remove(order);
    }

    public async Task<int> GetApiProductIdAsync(int ourProductId)
    {
        return (await _context.ApiProducts
            .Include(p => p.Product)
            .AsNoTracking()
            .Where(x => x.Product.Id == ourProductId).FirstAsync()).ApiProductId;
    }

    public async Task<int> GetApiOrderIdAsync(int ourOrderId)
    {
        return (await _context.ApiOrders
            .Include(p => p.Order)
            .AsNoTracking()
            .Where(x => x.Order.Id == ourOrderId).FirstAsync()).ApiOrderId;
    }

    public async Task<List<ApiOrder>> GetAllOrdersAsync()
    {
        return await _context.ApiOrders.Include(x => x.Order)
            .ToListAsync();
    }

    public async Task<List<ApiProduct>> GetAllProductsAsync()
    {
        return await _context.ApiProducts.Include(x => x.Product)
            .ToListAsync();
    }
}