using ChargingApp.Interfaces;

namespace ChargingApp.Controllers;

public class ProductAdminController : AdminController
{
    public ProductAdminController(IPhotoService photoService) : base(photoService)
    {
    }
}