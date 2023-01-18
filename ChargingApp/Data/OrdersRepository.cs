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

    public async Task<List<OrderAdminDto>> GetCanceledOrdersRequestAsync(string? userEmail = null)
    {
        if (userEmail != null)
            return await _context.Orders
                .Include(x => x.User)
                .Where(x => x.StatusIfCanceled == 1 && x.Status == 0)
                .Where(x => x.User.Email == userEmail.ToLower())
                .OrderByDescending(x => x.CreatedAt)
                .ProjectTo<OrderAdminDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

        return await _context.Orders
            .Include(x => x.User)
            .Where(x => x.StatusIfCanceled == 1 && x.Status == 0)
            .OrderByDescending(x => x.CreatedAt)
            .ProjectTo<OrderAdminDto>(_mapper.ConfigurationProvider)
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


    public async Task<List<PendingOrderDto>> GetPendingOrdersAsync(string email = "")
    {
        if (email == "")
            return await _context.Orders
                .Include(x => x.User)
                .OrderByDescending(x => x.CreatedAt)
                .Where(x => x.Status == 0)
                .ProjectTo<PendingOrderDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

        return await _context.Orders
            .Include(x => x.User)
            .Where(x => x.Status == 0)
            .Where(x => x.User.Email == email)
            .OrderByDescending(x => x.CreatedAt)
            .ProjectTo<PendingOrderDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<bool> CheckPendingOrdersForUserByEmailAsync(string email)
    {
        email = email.ToLower();

        var res = await _context.Orders
            .Include(x => x.User)
            .Where(x => x.Status == 0 && x.User.Email.ToLower() == email)
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

        return await query.ToListAsync();
    }
}