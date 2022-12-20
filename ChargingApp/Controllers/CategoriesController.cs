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
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUserRepository _userRepo;
    private readonly IVipLevelRepository _vipRepo;
    private readonly IPhotoService _photoService;
    private readonly IMapper _mapper;

    public CategoriesController(ICategoryRepository categoryRepository, IUserRepository userRepo
        , IVipLevelRepository vipRepo, IPhotoService photoService, IMapper mapper
    )
    {
        _categoryRepository = categoryRepository;
        _userRepo = userRepo;
        _vipRepo = vipRepo;
        _photoService = photoService;
        _mapper = mapper;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryResultDto?>> GetCategoryById(int id)
    {
        if (await _categoryRepository.GetCategoryByIdAsync(id) is null)
            return BadRequest(new ApiResponse(400, "This category isn't exist"));

        var res = new CategoryResultDto
        {
            Category = await _categoryRepository.GetCategoryByIdProjectedAsync(id)
        };

        var email = User.GetEmail();
        if (email is null)
            return Ok(new ApiOkResponse(res));

        var user = await _userRepo.GetUserByEmailAsync(email);
        if (user is null) return res;

        var discount = await _vipRepo.GetVipLevelDiscount(user.VIPLevel);
        for (int i = 0; i < res.Category.Products.Count; i++)
        {
            res.Category.Products[i].Price -= res.Category.Products[i].Price *
                (discount) / 100;
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

        _categoryRepository.AddCategory(category);

        if (await _categoryRepository.SaveAllAsync())
            return Ok(new ApiResponse(201, "Category added"));

        return BadRequest(new ApiResponse(400, "something went wrong"));
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteCategoty(int categoryId)
    {
        var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);

        if (category is null)
            return NotFound(new ApiResponse(403, "category not found"));

        var res = await _categoryRepository.DeleteCategoryByIdAsync(categoryId);

        if (res) return Ok(new ApiResponse(201, "Deleted successfully"));

        return BadRequest(new ApiResponse(400, "Failed to delete category"));
    }

    [HttpPut("{categoryId:int}")]
    public async Task<ActionResult> UpdateCategory(int categoryId, [FromForm] CategoryUpdateDto dto)
    {
        var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);

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

        _categoryRepository.UpdateCategory(category);

        await _categoryRepository.SaveAllAsync();
        return Ok();
    }
}