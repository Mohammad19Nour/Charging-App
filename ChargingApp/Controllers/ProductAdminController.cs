using ChargingApp.Interfaces;

namespace ChargingApp.Controllers;

public class ProductAdminController : AdminController
{
    public ProductAdminController(ICategoryRepository repo, IPhotoService photoService) 
        : base(repo, photoService)
    {
    }
}