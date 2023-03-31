using AutoMapper;
using ChargingApp.Entity;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace ChargingApp.Controllers;

public class ApproveOrderController :AdminOrderController
{
    
    public ApproveOrderController(IUnitOfWork unitOfWork, INotificationService notificationService, IMapper mapper, IApiService apiService, UserManager<AppUser> userManager) 
        : base(unitOfWork, notificationService, mapper, apiService, userManager)
    {
    }
}