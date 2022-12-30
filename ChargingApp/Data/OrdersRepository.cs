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

    public OrdersRepository(DataContext context,IMapper mapper)
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
        return await _context.Orders.FirstOrDefaultAsync(x => x.Id == orderId);
    }

    public async Task<List<NormalOrderDto>> GetNormalUserOrdersAsync(int userId)
    {
        return await _context.Orders
            .Where(p=>p.UserId==userId)
            .OrderByDescending(x=>x.CreatedAt)
            .ProjectTo<NormalOrderDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<List<OrderDto>> GetVipUserOrdersAsync(int userId)
    {
        return await _context.Orders
            .Where(p=>p.UserId==userId)
            .OrderByDescending(x=>x.CreatedAt)
            .ProjectTo<OrderDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<Order?> GetLastOrderForUserByIdAsync(int userId)
    {
        var order = await _context.Orders
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        return order; 
    }
}