using ChargingApp.Interfaces;

namespace ChargingApp.Controllers;

public class AdminVipLevelController : AdminController
{
    private readonly IUnitOfWork _unitOfWork;

    public AdminVipLevelController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
}