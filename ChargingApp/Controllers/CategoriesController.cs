using AutoMapper;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Extentions;
using ChargingApp.Helpers;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

public class CategoriesController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPhotoService _photoService;
    private readonly IMapper _mapper;

    public CategoriesController(IUnitOfWork unitOfWork, IPhotoService photoService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _photoService = photoService;
        _mapper = mapper;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryResultDto?>> GetCategoryById(int id)
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

        if (user != null) vipLevel = user.VIPLevel;


        res.Category.Products = await SomeUsefulFunction.CalcPriceForProducts
            (user, res.Category.Products, _unitOfWork, vipLevel);

        return Ok(new ApiOkResponse(res));
    }
}