using AutoMapper;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Extentions;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChargingApp.SignalR;
[Authorize]
public class PresenceHub : Hub
{
    private readonly PresenceTracker _tracker;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PresenceHub(PresenceTracker tracker , IUnitOfWork unitOfWork , IMapper mapper)
    {
        _tracker = tracker;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public override async Task OnConnectedAsync()
    {
        string? email = null;
        if (Context.User != null)
        {
            email = Context.User.GetEmail();
        }
        else 
            throw new HubException("unauthorized..");
        
        await _tracker.UserConnected(email,Context.ConnectionId);
        Console.WriteLine(Context.ConnectionId+"\n");
        await Clients.Caller.SendAsync("User", "connected");

    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
       // Console.WriteLine("**\n");
        string? email = null;
        
        if (Context.User != null)
        {
            email = Context.User.GetEmail();
        }
        if (email is null) throw new HubException("unauthorized..");

        await _tracker.UserDisConnected(Context.User.GetEmail(), Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
        
    }

    public async Task<List<OrderDto>> GetOrderNotifications()
    {
        string? email = null;
        if (Context.User != null)
        {
            email = Context.User.GetEmail();
        }

        if (email is null)
            throw new HubException("unauthorized..");

        var res =
            await _unitOfWork.NotificationRepository.GetNotificationForUserByEmailAsync(email);
      
        res = res.Where(x => x.Order != null).ToList();
        var tmp = new List<OrderDto>();
         res.ForEach(x => tmp.Add(_mapper.Map<OrderDto>(x.Order)));

         
         if (tmp.Count == 0)
             return new List<OrderDto>();

         foreach (var t in res)
         {
             _unitOfWork.NotificationRepository.DeleteNotification(t);
         }

         if (await _unitOfWork.Complete())
         {
            return tmp;
         }
         throw new HubException("Failed to fetch notifications");
    }
    
    public async Task<List<PaymentDto>> GetPaymentNotifications()
    {
        string? email = null;
        if (Context.User != null)
        {
            email = Context.User.GetEmail();
        }

        if (email is null)
            throw new HubException("unauthorized..");

        var res =
            await _unitOfWork.NotificationRepository.GetNotificationForUserByEmailAsync(email);
        
        res = res.Where(x => x.Payment != null).ToList();
        var tmp = new List<PaymentDto>();
        res.ForEach(x => tmp.Add(_mapper.Map<PaymentDto>(x.Payment)));

        if (tmp.Count == 0)
            return new List<PaymentDto>();
        
        foreach (var t in res)
        {
            _unitOfWork.NotificationRepository.DeleteNotification(t);
        }

        if (await _unitOfWork.Complete())
        {
            return tmp;
        }
        throw new HubException("Failed to fetch notifications");
    }
    public async Task<string> Messs(string d)
    {
        return "gg";
    }
}