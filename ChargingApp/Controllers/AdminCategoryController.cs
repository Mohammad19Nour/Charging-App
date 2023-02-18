using AutoMapper;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

public class AdminCategoryController : AdminController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPhotoService _photoService;
    private readonly IMapper _mapper;

    public AdminCategoryController(IUnitOfWork unitOfWork, IPhotoService photoService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _photoService = photoService;
        _mapper = mapper;
    }

    [Authorize(Policy = "Required_Admins_Role")]
    [HttpPost("add-category")]
    public async Task<ActionResult> AddCategory([FromForm] NewCategoryDto dto)
    {
        try
        {
            if (dto.ImageFile is null)
                return BadRequest(new ApiResponse(400, "image file is null"));

            var result = await _photoService.AddPhotoAsync(dto.ImageFile);

            if (!result.Success)
                return BadRequest(new ApiResponse(400, "Failed to upload photo  " + result.Message));

            var category = new Category
            {
                EnglishName = dto.EnglishName,
                ArabicName = dto.ArabicName,
                HasSubCategories = dto.HasSubCategories,
                Photo = new Photo { Url = result.Url }
            };

            _unitOfWork.CategoryRepository.AddCategory(category);

            if (await _unitOfWork.Complete())
                return Ok(new ApiResponse(201, "Category added"));

            return BadRequest(new ApiResponse(400, "something went wrong"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [Authorize(Policy = "Required_Admins_Role")]
    [HttpDelete]
    public async Task<ActionResult> DeleteCategory(int categoryId)
    {
        try
        {
            var category = await _unitOfWork.CategoryRepository.GetCategoryByIdAsync(categoryId);

            if (category is null)
                return NotFound(new ApiResponse(403, "category not found"));

            var name = category.Photo?.Url;

            if (category.Products != null)
                foreach (var t in category.Products)
                {
                    var y = await _unitOfWork.ProductRepository.GetProductByIdAsync(t.Id);
                    _unitOfWork.ProductRepository.DeleteProductFromCategory(y);
                }

            _unitOfWork.CategoryRepository.DeleteCategory(category);
            var tmp = category.Photo != null &&
                      await _unitOfWork.PhotoRepository.DeletePhotoByIdAsync(category.Photo.Id);

            if (!_unitOfWork.HasChanges() || !tmp) return BadRequest(new ApiResponse(400, "Failed to delete category"));

            if (!await _unitOfWork.Complete()) return BadRequest(new ApiResponse(400, "Failed to update photo"));
               
            if (name != null) await _photoService.DeletePhotoAsync(name);

            return Ok(new ApiResponse(201, "Deleted successfully"));

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [Authorize(Policy = "Required_Admins_Role")]
    [HttpPut("update-cat-name/{categoryId:int}")]
    public async Task<ActionResult> UpdateCategory(int categoryId, [FromBody] CategoryUpdateDto dto)
    {
        try
        {
            var category = await _unitOfWork.CategoryRepository.GetCategoryByIdAsync(categoryId);

            if (category is null)
                return NotFound(new ApiResponse(404, "category not found"));

            if (!string.IsNullOrEmpty(dto.ArabicName))
                category.ArabicName = dto.ArabicName;

            if (!string.IsNullOrEmpty(dto.EnglishName))
                category.EnglishName = dto.EnglishName;

            _unitOfWork.CategoryRepository.UpdateCategory(category);

            if (await _unitOfWork.Complete())
                return Ok(new ApiResponse(200));
            return BadRequest(new ApiResponse(400, "something went wrong"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [Authorize(Policy = "Required_AllAdminExceptNormal_Role")]
    [HttpPut("update-cat-photo/{categoryId:int}")]
    public async Task<ActionResult> UpdatePhoto(int categoryId, IFormFile? imageFile)
    {
        var category = await _unitOfWork.CategoryRepository.GetCategoryByIdAsync(categoryId);

        if (category is null)
            return NotFound(new ApiResponse(404, "category not found"));

        if (imageFile == null) return BadRequest(new ApiResponse(400, "image file is required"));


        var result = await _photoService.AddPhotoAsync(imageFile);

        if (!result.Success)
            return BadRequest(new ApiResponse(400, result.Message));

        category.Photo.Url = result.Url;

        if (await _unitOfWork.Complete())
        {
            return Ok(new ApiResponse(200, "File uploaded successfully."));
        }

        return BadRequest(new ApiResponse(400, "Failed"));
    }
}