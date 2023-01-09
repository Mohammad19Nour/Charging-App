using AutoMapper;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Extentions;
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

        var syria = await _unitOfWork.CurrencyRepository.GetSyrianCurrency();
        var turkey = await _unitOfWork.CurrencyRepository.GetSyrianCurrency();

        var email = User.GetEmail();
        if (email is null)
            return Ok(new ApiOkResponse(res));

        var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(email);
        if (user is null) return Ok(new ApiOkResponse(res));

        var discount = await _unitOfWork.VipLevelRepository.GetVipLevelDiscount(user.VIPLevel);

        foreach (var t in res.Category.Products)
        {
            t.Price -= t.Price *
                (discount) / 100;
            
            t.TurkishPrice = t.Price * turkey;
            t.SyrianPrice = t.Price * syria;
        }

        return Ok(new ApiOkResponse(res));
    }

    [HttpPost("add-category")]
    public async Task<ActionResult> AddCategory([FromForm] NewCategoryDto dto)
    {
        //Console.WriteLine(photoFile.Length);
        var category = new Category
        {
            EnglishName = dto.EnglishName,
            ArabicName = dto.ArabicName,
            HasSubCategories = dto.HasSubCategories
        };
        var result = await _photoService.AddPhotoAsync(dto.ImageFile);
        if (result.Error != null)
            return BadRequest(new ApiResponse(400, result.Error.Message));

        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };

        category.Photo = photo;

        _unitOfWork.CategoryRepository.AddCategory(category);

        if (await _unitOfWork.Complete())
            return Ok(new ApiResponse(201, "Category added"));

        return BadRequest(new ApiResponse(400, "something went wrong"));
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteCategory(int categoryId)
    {
        var category = await _unitOfWork.CategoryRepository.GetCategoryByIdAsync(categoryId);

        if (category is null)
            return NotFound(new ApiResponse(403, "category not found"));

        _unitOfWork.CategoryRepository.DeleteCategory(category);

        if (await _unitOfWork.Complete()) return Ok(new ApiResponse(201, "Deleted successfully"));

        return BadRequest(new ApiResponse(400, "Failed to delete category"));
    }

    [HttpPut("{categoryId:int}")]
    public async Task<ActionResult> UpdateCategory(int categoryId, [FromForm] CategoryUpdateDto dto)
    {
        var category = await _unitOfWork.CategoryRepository.GetCategoryByIdAsync(categoryId);

        if (category is null)
            return NotFound(new ApiResponse(404, "category not found"));


        if (dto.EnglishName != null && dto.EnglishName.Length > 0) category.EnglishName = dto.EnglishName;
        if (dto.ArabicName != null && dto.ArabicName.Length > 0) category.ArabicName = dto.ArabicName;

        if (dto.ImageFile != null)
        {
            try
            {
                var result = await _photoService.AddPhotoAsync(dto.ImageFile);
                if (result.Error != null)
                    return BadRequest(new ApiResponse(400, result.Error.Message));

                category.Photo.Url = result.SecureUrl.AbsoluteUri;
                category.Photo.PublicId = result.PublicId;
            }
            catch (Exception e)
            {
                throw new Exception("Failed to update image");
            }
        }

        _unitOfWork.CategoryRepository.UpdateCategory(category);

        await _unitOfWork.Complete();
        return Ok();
    }
}