using ChargingApp.DTOs;
using ChargingApp.Errors;
using ChargingApp.Extentions;
using ChargingApp.Helpers;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

public class CategoriesController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoriesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryResultDto?>> GetCategoryById(int id)
    {
        try
        {
            if (await _unitOfWork.CategoryRepository.GetCategoryByIdAsync(id) is null)
                return BadRequest(new ApiResponse(400, "This category isn't exist"));

            var res = new CategoryResultDto
            {
                Category = await _unitOfWork.CategoryRepository.GetCategoryByIdProjectedAsync(id)
            };

            var vipLevel = 0;
            var email = User.GetEmail();
            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(email);

            if (res.Category is null || res.Category.Products is null)
                return Ok(new ApiOkResponse(new CategoryResultDto()));
            if (user != null)
            {
                vipLevel = user.VIPLevel;
            }


            res.Category.Products = await SomeUsefulFunction.CalcPriceForProducts
                (user, res.Category.Products, _unitOfWork, vipLevel);

            return Ok(new ApiOkResponse(res));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}