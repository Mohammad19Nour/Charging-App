using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

public class AdminCategoryController : AdminController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPhotoService _photoService;

    public AdminCategoryController(IUnitOfWork unitOfWork , IPhotoService photoService)
    {
        _unitOfWork = unitOfWork;
        _photoService = photoService;
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