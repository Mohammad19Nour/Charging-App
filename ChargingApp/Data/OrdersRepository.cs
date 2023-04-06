using AutoMapper;
using AutoMapper.QueryableExtensions;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChargingApp.Data;

public class OrdersRepository : IOrdersRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public OrdersRepository(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public void AddOrder(Order order)
    {
        _context.Orders.Add(order);
        // return await SaveAllChangesAsync();
    }

    public async Task<bool> DeleteOrderByIdAsync(int orderId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order is null) return false;
        _context.Remove(order);
        return true;
    }

    public void DeleteOrder(Order order)
    {
        _context.Orders.Remove(order);
    }

    public async Task<Order?> GetOrderByIdAsync(int orderId)
    {
        return await _context.Orders
            .Include(u => u.User)
            .Include(x => x.Product)
            .Include(x => x.Photo)
            .FirstOrDefaultAsync(x => x.Id == orderId);
    }

    public async Task<List<NormalOrderDto>> GetNormalUserOrdersAsync(int userId)
    {
        return await _context.Orders
            .Where(p => p.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ProjectTo<NormalOrderDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<List<OrderDto>> GetVipUserOrdersAsync(int userId)
    {
        return await _context.Orders
            .Where(p => p.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ProjectTo<OrderDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<Order?> GetLastOrderForUserByIdAsync(int userId)
    {
        var order = await _context.Orders
            .Where(p => p.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        return order;
    }


    public async Task<List<Order>> GetPendingOrdersAsync(string email = "")
    {
        email = email.ToLower();

        var res = _context.Orders
            .Include(x => x.Photo)
            .Include(x => x.User)
            .Include(x => x.Product)
             .OrderByDescending(x => x.CreatedAt)
            .Where(x => x.Status == 0 || x.Status == 4);

        if (!string.IsNullOrEmpty(email))
            res = res.Where(x => x.User.Email == email);


        res = res.OrderBy(x => x.CreatedAt);
        var ret = await res.ToListAsync();
        return ret;
    }

    public async Task<bool> CheckPendingOrdersForUserByEmailAsync(string email)
    {
        email = email.ToLower();

        var res = await _context.Orders
            .Include(x => x.User)
            .AsNoTracking()
            .Where(x => x.Status == 0 || x.Status == 4)
            .Where(x => x.User.Email.ToLower() == email)
            .OrderByDescending(x=>x.CreatedAt)
            .FirstOrDefaultAsync();

        return res != null;
    }

    public async Task<List<Order>> GetDoneOrders(DateQueryDto dto, string? userEmail = null)
    {
        var query = _context.Orders
            .Where(x => x.Status == 1)
            .Include(x => x.User)
            .Include(x => x.Photo)
            .AsQueryable();

        if (dto.Day != null)
            query = query.Where(x => x.CreatedAt.Day == dto.Day);
        if (dto.Month != null)
            query = query.Where(x => x.CreatedAt.Month == dto.Month);
        if (dto.Year != null)
            query = query.Where(x => x.CreatedAt.Year == dto.Year);
        if (userEmail != null)
            query = query.Where(x => x.User.Email == userEmail);

        query = query.OrderByDescending(x => x.CreatedAt);

        return await query.ToListAsync();
    }

    public async Task<List<Order?>> GetOrdersForSpecificProduct(int productId)
    {
        return await _context.Orders
            .Include(x=>x.Product)
            .Where(x => x.Product.Id == productId).ToListAsync();
    }
}