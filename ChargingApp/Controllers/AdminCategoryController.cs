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
    [HttpGet("category/{id:int}")]
    public async Task<ActionResult<CategoryResultDto>> GetCategory(int id)
    {
        var category = await _unitOfWork.CategoryRepository.GetCategoryByIdAsync(id);

        if (category == null)
            return BadRequest(new ApiResponse(400, "Can't find category with id " + id));
        return new CategoryResultDto
        {
            Category = await _unitOfWork.CategoryRepository.GetCategoryByIdProjectedAsync(id)
        };
    }

    [Authorize(Policy = "Required_Admins_Role")]
    [HttpPost("add-category")]
    public async Task<ActionResult> AddCategory([FromForm] NewCategoryDto dto)
    {
        try
        {
            if (dto.ImageFile is null)
                return BadRequest(new ApiResponse(400, "Image file is required"));

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

            foreach (var t in category.Products)
            {
                var ordersForThisProduct = await _unitOfWork.OrdersRepository
                    .GetOrdersForSpecificProduct(t.Id);

                if (ordersForThisProduct.Any(x => x.Status is 0 or 4))
                    return BadRequest(new ApiResponse(400, "There are pending orders for for some products in this category..."));

                var fromOtherApi = await _unitOfWork.OtherApiRepository
                    .CheckIfProductExistAsync(t.Id, true);

                foreach (var x in ordersForThisProduct)
                {
                    x!.Product = null;
                }

                if (fromOtherApi)
                    _unitOfWork.OtherApiRepository.DeleteProduct(t.Id);

                _unitOfWork.ProductRepository.DeleteProductFromCategory(t);
            }

            _unitOfWork.CategoryRepository.DeleteCategory(category);

            if (!await _unitOfWork.Complete()) return BadRequest(new ApiResponse(400, "Failed to delete category"));

            if (category.Photo != null &&
                await _unitOfWork.PhotoRepository.DeletePhotoByIdAsync(category.Photo.Id))
            {
                await _unitOfWork.Complete();
            }

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
            return Ok(new ApiResponse(200, "Image updated successfully."));
        }

        return BadRequest(new ApiResponse(400, "Failed to update the image... try again"));
    }
}