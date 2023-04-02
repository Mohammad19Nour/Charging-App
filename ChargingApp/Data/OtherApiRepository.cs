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
            .Include(x => x.HostingSite)
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
            .Include(x => x.Product)
            .FirstOrDefault(x => x.Product.Id == productId);

        if (product != null)
            _context.ApiProducts.Remove(product);
    }

    public void DeleteOrder(int orderId)
    {
        var order = _context.ApiOrders
            .Include(x => x.Order)
            .First(x => x.Order.Id == orderId);
        _context.ApiOrders.Remove(order);
    }

    public async Task<int> GetApiProductIdAsync(int ourProductId)
    {
        return (await _context.ApiProducts
            .Include(x => x.HostingSite)
            .Include(p => p.Product)
            .AsNoTracking()
            .Where(x => x.Product.Id == ourProductId).FirstAsync()).ApiProductId;
    }

    public async Task<int> GetApiOrderIdAsync(int ourOrderId)
    {
        return (await _context.ApiOrders
            .Include(p => p.Order)
            .Include(x => x.HostingSite)
            .AsNoTracking()
            .Where(x => x.Order.Id == ourOrderId).FirstAsync()).ApiOrderId;
    }

    public async Task<List<ApiOrder>> GetAllOrdersAsync()
    {
        return await _context.ApiOrders
            .Include(x => x.HostingSite)
            .Include(x => x.Order)
            .ToListAsync();
    }

    public async Task<List<ApiProduct>> GetAllProductsAsync()
    {
        return await _context.ApiProducts
            .Include(x => x.HostingSite)
            .Include(x => x.Product)
            .ToListAsync();
    }

    public async Task<ApiProduct> GetProductByOurIdAsync(int id)
    {
        return await _context.ApiProducts
            .Include(x => x.Product)
            .Include(x => x.HostingSite)
            .Where(x => x.Product.Id == id)
            .FirstAsync();
    }

    public async Task<ApiOrder> GetOrderByOurIdAsync(int id)
    {
        return await _context.ApiOrders
            .Include(x => x.Order)
            .Include(x => x.HostingSite)
            .Where(x => x.Order.Id == id)
            .Where(x => x.Order.Id == id)
            .FirstAsync();
    }

    public async Task<HostingSite?> GetHostingSiteByNameAsync(string? name)
    {
        if (string.IsNullOrEmpty(name)) return null;

        name = name.ToLower();
        return await _context.HostingSites
            .Where(x => x.SiteName.ToLower() == name)
            .FirstOrDefaultAsync();
    }

    public async Task<List<HostingSite>> GetAllHostingSiteAsync()
    {
        return await _context.HostingSites.ToListAsync();
    }

    public void UpdateHostingSite(HostingSite site)
    {
        _context.HostingSites.Update(site);
    }
}