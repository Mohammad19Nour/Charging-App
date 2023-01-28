using AutoMapper;
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
    private readonly IMapper _mapper;

    public AdminProductController(IUnitOfWork unitOfWork, IPhotoService photoService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _photoService = photoService;
        _mapper = mapper;
    }

    [HttpPost("add-product-no-quantity/{categoryId:int}")]
    public async Task<ActionResult> AddProduct(int categoryId, [FromForm] NewProductDto dto)
    {
        var category = await _unitOfWork.CategoryRepository.GetCategoryByIdAsync(categoryId);

        if (category is null)
            return NotFound(new ApiResponse(404, "category not found"));

        if (!category.HasSubCategories)
            return BadRequest(new ApiResponse(400, "you can't add this product to this category"));

        var result = await _photoService.AddPhotoAsync(dto.PhotoFile);
        if (!result.Success)
            return BadRequest(new ApiResponse(400, result.Message));

        var photo = new Photo
        {
            Url = result.Url
        };

        var product = new Product
        {
            EnglishName = dto.EnglishName,
            ArabicName = dto.ArabicName,
            CanChooseQuantity = false,
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

    [HttpPost("add-product-with-quantity/{categoryId:int}")]
    public async Task<ActionResult> AddProductWithQuantity(int categoryId, [FromForm] NewProductWithQuantityDto dto)
    {
        var category = await _unitOfWork.CategoryRepository.GetCategoryByIdAsync(categoryId);

        if (category is null)
            return NotFound(new ApiResponse(404, "category not found"));

        if (category.HasSubCategories)
            return BadRequest(new ApiResponse(400, "you can't add this product to this category"));

        if (dto.PriceList.Count != dto.QuantityList.Count || dto.PriceList.Count == 0)
            return BadRequest(new ApiResponse(400, "price and quantity lists should be with the same size"));

        using var quantity = dto.QuantityList.GetEnumerator();
        using var price = dto.PriceList.GetEnumerator();
        
        foreach (var t in dto.PriceList)
        {
            quantity.MoveNext();
            price.MoveNext();
            var product = new Product
            {
                EnglishName = category.EnglishName,
                ArabicName = category.ArabicName,
                CanChooseQuantity = true,
                Category = category,
                Price = price.Current,

                Quantity = quantity.Current,
            };
            
            _unitOfWork.ProductRepository.AddProduct(product);
        }
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

        var orders = await _unitOfWork.OrdersRepository.GetPendingOrdersAsync();
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

        var properties = !product.CanChooseQuantity ? typeof(ProductToUpdateDto).GetProperties() : typeof(ProductWithQuantityToUpdateDto).GetProperties();

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
        try
        {
            var product = await _unitOfWork.ProductRepository.GetProductByIdAsync(productId);

            if (product is null)
                return BadRequest(new ApiResponse(400, "this product isn't exist"));

            if (product.CanChooseQuantity)
                return BadRequest(new ApiResponse(400,"can't update photo for this product"));

            var result = await _photoService.AddPhotoAsync(file);
            if (!result.Success)
                return BadRequest(new ApiResponse(400, result.Message));

            var photo = new Photo
            {
                Url = result.Url
            };
            product.Photo ??= new Photo();
            product.Photo = photo;
            
            if (await _unitOfWork.Complete())
                return Ok(new ApiResponse(200, "image updated"));
            
            return BadRequest(new ApiResponse(400, "Failed to update image"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(new ApiResponse(400, "Something went wrong"));
        }
    }

    [HttpGet("product/{prodictId:int}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int productId)
    {
        var product = await _unitOfWork.ProductRepository.GetProductByIdAsync(productId);

        if (product is null)
            return BadRequest(new ApiResponse(400, "this product isn't exist"));

        if (product.CanChooseQuantity)
            return BadRequest(new ApiResponse(400,"can't update photo for this product"));

        return Ok(new ApiOkResponse(_mapper.Map<ProductDto>(product)));
    }
}