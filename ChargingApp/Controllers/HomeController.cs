using ChargingApp.DTOs;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Mvc;
namespace ChargingApp.Controllers;

public class HomeController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;

    public HomeController(IUnitOfWork unitOfWork
    )
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<ActionResult<HomeDto>> GetHomePage()
    {
        try
        {
            var res = new HomeDto
            {
                Categories = await _unitOfWork.CategoryRepository.GetAllCategoriesAsync()
            };
            return res;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}