using System.Reflection;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

public class AdminProductController : AdminController
{
     private readonly IUnitOfWork _unitOfWork;
    private readonly IPhotoService _photoService;

    public AdminProductController(IUnitOfWork unitOfWork, IPhotoService photoService)
    {
        _unitOfWork = unitOfWork;
        _photoService = photoService;
    }

    [HttpPost("add-product/{categoryId:int}")]
    public async Task<ActionResult> AddProduct(int categoryId, [FromForm] NewProductDto dto)
    {
        var category = await _unitOfWork.CategoryRepository.GetCategoryByIdAsync(categoryId);

        if (category is null)
            return NotFound(new ApiResponse(404, "category not found"));

        if (dto.CanChooseQuantity)
        {
            return await AddNewProductWithQuantity(dto, category);
        }

        var result = await _photoService.AddPhotoAsync(dto.PhotoFile);

        if (result.Error != null) return BadRequest(new ApiResponse(400, "Failed to upload photo"));

        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };
        var product = new Product
        {
            EnglishName = dto.EnglishName,
            ArabicName = dto.ArabicName,
            CanChooseQuantity = dto.CanChooseQuantity,
            Price = dto.Price,
            Category = category,
            Photo = photo,
            MinimumQuantityAllowed = dto.MinimumQuantityAllowed
        };

        _unitOfWork.ProductRepository.AddProduct(product);

        if (await _unitOfWork.Complete())
            return Ok(new ApiResponse(200, "product added successfully"));
        return BadRequest(new ApiResponse(400, "Failed to add product"));
    }

    [HttpDelete("{productId:int}")]
    public async Task<ActionResult> DeleteProduct(int productId)
    {
        var product = await _unitOfWork.ProductRepository.GetProductByIdAsync(productId);

        if (product is null)
            return NotFound(new ApiResponse(403, "this product isn't exist"));

        _unitOfWork.ProductRepository.DeleteProductFromCategory(product);
        
        if (await _unitOfWork.Complete())
            return Ok(new ApiResponse(201, "product deleted"));

        return BadRequest(new ApiResponse(400, "Failed to delete product"));
    }

    [HttpPatch("update-info/{productId:int}")]
    public async Task<ActionResult> UpdateProduct(int productId, JsonPatchDocument patch)
    {
        var product = await _unitOfWork.ProductRepository.GetProductByIdAsync(productId);

        if (product is null)
            return BadRequest(new ApiResponse(400, "this product isn't exist"));

        var list = patch.Operations.Select(x => x.path.ToLower());
        PropertyInfo[] properties = typeof(ProductToUpdateDto).GetProperties();

        var propertiesName = properties.Select(x => x.Name.ToLower()).ToList();

        foreach (var path in list)
        {
            if (propertiesName.FirstOrDefault(x => x == path) is null)
                return BadRequest(new ApiResponse(400, path + " property isn't exist"));
        }

        patch.ApplyTo(product);

        if (await _unitOfWork.Complete())
            return Ok(new ApiResponse(200, "updated successfully"));
        return BadRequest(new ApiResponse(400, "Failed to update product"));
    }

    [HttpPut("update-photo/{productId:int}")]
    public async Task<ActionResult> UpdateProductPhoto(int productId, IFormFile file)
    {
        var product = await _unitOfWork.ProductRepository.GetProductByIdAsync(productId);

        if (product is null)
            return BadRequest(new ApiResponse(400, "this product isn't exist"));

        try
        {
            var result = await _photoService.AddPhotoAsync(file);
            if (result.Error != null)
                return BadRequest(new ApiResponse(400, result.Error.Message));

            product.Photo ??= new Photo();
            product.Photo.Url = result.SecureUrl.AbsoluteUri;
            product.Photo.PublicId = result.PublicId;
        }
        catch (Exception e)
        {
            throw new Exception("Failed to update image");
        }

        return Ok(new ApiResponse(200, "image updated"));
    }

    private async Task<ActionResult> AddNewProductWithQuantity(NewProductDto dto, Category category)
    {
        if (category.HasSubCategories)
            return BadRequest(new ApiResponse(400, "you can't add this product to this category"));

        var result = await _photoService.AddPhotoAsync(dto.PhotoFile);

        if (result.Error != null) return BadRequest(new ApiResponse(400, "Failed to upload photo"));

        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };
        var product = new Product
        {
            EnglishName = dto.EnglishName,
            ArabicName = dto.ArabicName,
            CanChooseQuantity = dto.CanChooseQuantity,
            Price = dto.Price,
            Category = category,
            Photo = photo,
            MinimumQuantityAllowed = dto.MinimumQuantityAllowed,
        };

        product.AvailableQuantities =
            dto.AvailableQuantities.Select(x => new Quantity { Value = x, Product = product }).ToList();

        _unitOfWork.ProductRepository.AddProduct(product);

        if (await _unitOfWork.Complete())
            return Ok(new ApiResponse(201, "Product added"));
        return BadRequest(new ApiException(400, "Failed to add product"));
    }

}