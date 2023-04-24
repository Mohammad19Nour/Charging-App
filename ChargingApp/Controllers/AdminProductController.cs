using AutoMapper;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using ChargingApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

public class AdminProductController : AdminController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPhotoService _photoService;
    private readonly IMapper _mapper;
    private readonly IApiService _apiService;

    public AdminProductController(IUnitOfWork unitOfWork, IPhotoService photoService,
        IMapper mapper, IApiService apiService)
    {
        _unitOfWork = unitOfWork;
        _photoService = photoService;
        _mapper = mapper;
        _apiService = apiService;
    }

    [Authorize(Policy = "Required_AllAdminExceptNormal_Role")]
    [HttpPost("add-product-no-quantity/{categoryId:int}")]
    public async Task<ActionResult> AddProduct(int categoryId, [FromForm] NewProductDto dto
        , [FromQuery] int? productId, [FromQuery] string? siteName)
    {
        try
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
            if (productId != null)
            {
                siteName = siteName.ToLower();
                var hostingSite = await _unitOfWork.OtherApiRepository
                    .GetHostingSiteByNameAsync(siteName);

                Console.WriteLine(siteName);
                if (hostingSite is null)
                    return BadRequest(new ApiResponse(400, "" +
                                                           "Site not found"));

                var res = await _apiService.CheckProductByIdIfExistAsync((int)productId,
                    hostingSite.BaseUrl, hostingSite.Token
                );

                if (!res)
                    return NotFound(new ApiResponse(404, "Product not found"));

                if (await _unitOfWork.OtherApiRepository
                        .CheckIfProductExistAsync(productId.Value, false))
                    return BadRequest(new ApiResponse(400, "Product already exist"));

                _unitOfWork.OtherApiRepository.AddProduct(new ApiProduct
                {
                    Product = product,
                    ApiProductId = productId.Value,
                    HostingSite = hostingSite
                });
            }

            if (await _unitOfWork.Complete())
                return Ok(new ApiResponse(200, "product added successfully"));
            return BadRequest(new ApiResponse(400, "Failed to add product"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [Authorize(Policy = "Required_AllAdminExceptNormal_Role")]
    [HttpPost("add-product-with-quantity/{categoryId:int}")]
    public async Task<ActionResult> AddProductWithQuantity(int categoryId, [FromForm] NewProductWithQuantityDto dto)
    {
        try
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

            quantity.MoveNext();
            price.MoveNext();

            foreach (var t in dto.PriceList)
            {
                var product = new Product
                {
                    EnglishName = category.EnglishName,
                    ArabicName = category.ArabicName,
                    CanChooseQuantity = true,
                    Category = category,
                    Price = price.Current,
                    Quantity = quantity.Current,
                };
                if (product.Price != 0 || product.Quantity != 0)
                    _unitOfWork.ProductRepository.AddProduct(product);
                quantity.MoveNext();
                price.MoveNext();
            }

            if (await _unitOfWork.Complete())
                return Ok(new ApiResponse(200, "product added successfully"));
            return BadRequest(new ApiResponse(400, "Failed to add product"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [Authorize(Policy = "Required_Admins_Role")]
    [HttpDelete("{productId:int}")]
    public async Task<ActionResult> DeleteProduct(int productId)
    {
        try
        {
            var product = await _unitOfWork.ProductRepository.GetProductByIdAsync(productId);

            if (product is null)
                return NotFound(new ApiResponse(403, "this product isn't exist"));

            var ordersForThisProduct = await _unitOfWork.OrdersRepository
                .GetOrdersForSpecificProduct(productId);
            
            if (ordersForThisProduct.Any(x => x.Status is 0 or 4))
                return BadRequest(new ApiResponse(400, "There are pending orders for this product..."));

            var fromOtherApi = await _unitOfWork.OtherApiRepository
                .CheckIfProductExistAsync(productId, true);
            
            foreach (var t in ordersForThisProduct)
            {
                t!.Product = null;
            }

            if (fromOtherApi)
                _unitOfWork.OtherApiRepository.DeleteProduct(productId);
            _unitOfWork.ProductRepository.DeleteProductFromCategory(product);

            if (await _unitOfWork.Complete())
                return Ok(new ApiResponse(201, "product deleted"));

            return BadRequest(new ApiResponse(400, "Failed to delete product"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [Authorize(Policy = "Required_Admins_Role")]
    [HttpPut("update-info/{productId:int}")]
    public async Task<ActionResult> UpdateProduct(int productId, ProductToUpdateDto dto)
    {
        var product = await _unitOfWork.ProductRepository.GetProductByIdAsync(productId);

        if (product is null)
            return BadRequest(new ApiResponse(400, "this product isn't exist"));

        if (product.CanChooseQuantity)
            return BadRequest(new ApiResponse(400, "can't update product with qty"));

        if (dto.Price != null)
            product.Price = (decimal)dto.Price;

        if (dto.Available != null)
            product.Available = (bool)dto.Available;

        if (dto.MinimumQuantityAllowed != null)
            product.MinimumQuantityAllowed = (int)dto.MinimumQuantityAllowed;

        if (!string.IsNullOrEmpty(dto.ArabicName))
            product.ArabicName = dto.ArabicName;

        if (!string.IsNullOrEmpty(dto.EnglishName))
            product.EnglishName = dto.EnglishName;

        _unitOfWork.ProductRepository.UpdateProduct(product);

        if (await _unitOfWork.Complete())
            return Ok(new ApiResponse(200, "updated successfully"));
        return BadRequest(new ApiResponse(400, "Failed to update product"));
    }

    [Authorize(Policy = "Required_Admins_Role")]
    [HttpPut("update-info-qty/{productId:int}")]
    public async Task<ActionResult> UpdateProductWithQuantity(int productId, ProductWithQuantityToUpdateDto dto)
    {
        var product = await _unitOfWork.ProductRepository.GetProductByIdAsync(productId);

        if (product is null)
            return BadRequest(new ApiResponse(400, "this product isn't exist"));

        if (!product.CanChooseQuantity)
            return BadRequest(new ApiResponse(400, "can't update product without qty"));

        if (dto.Price != null)
            product.Price = (decimal)dto.Price;

        if (dto.Quantity != null)
            product.Quantity = (decimal)dto.Quantity;

        if (dto.Available != null)
            product.Available = (bool)dto.Available;

        _unitOfWork.ProductRepository.UpdateProduct(product);

        if (await _unitOfWork.Complete())
            return Ok(new ApiResponse(200, "updated successfully"));
        return BadRequest(new ApiResponse(400, "Failed to update product"));
    }

    [Authorize(Policy = "Required_Admin1-Adv_Role")]
    [HttpPost("update-photo/{productId:int}")]
    public async Task<ActionResult> UpdateProductPhoto(int productId, IFormFile file)
    {
        try
        {
            var product = await _unitOfWork.ProductRepository.GetProductByIdAsync(productId);

            if (product is null)
                return BadRequest(new ApiResponse(400, "this product isn't exist"));

            if (product.CanChooseQuantity)
                return BadRequest(new ApiResponse(400, "can't update photo for this product"));

            var result = await _photoService.AddPhotoAsync(file);
            if (!result.Success)
                return BadRequest(new ApiResponse(400, result.Message));

            product.Photo ??= new Photo();
            product.Photo.Url = result.Url;
            _unitOfWork.ProductRepository.UpdateProduct(product);

            if (await _unitOfWork.Complete())
                return Ok(new ApiResponse(200, "image updated"));

            return BadRequest(new ApiResponse(400, "Failed to update image"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpGet("product/{productId:int}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int productId)
    {
        try
        {
            var product = await _unitOfWork.ProductRepository.GetProductByIdAsync(productId);

            if (product is null)
                return BadRequest(new ApiResponse(400, "this product isn't exist"));

            return Ok(new ApiOkResponse<ProductDto>(_mapper.Map<ProductDto>(product)));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [Authorize(Policy = "Required_AllAdminExceptNormal_Role")]
    [HttpGet("products-from-api")]
    public async Task<ActionResult> GetProductsFromApi()
    {
        try
        {
            var hostingSites = await _unitOfWork.OtherApiRepository.GetAllHostingSiteAsync();

            var list =
                new List<Dictionary<string, object?>>();

            foreach (var site in hostingSites)
            {
                var res = await _apiService
                    .GetAllProductsAsync(site.BaseUrl, site.Token);
                list.Add(new Dictionary<string, object?>
                {
                    { "site name", site.SiteName },
                    { "status", res.Message },
                    { "products", res.Status ? res.Products : new List<ApiService.ProductResponse>() }
                });
            }

            return Ok(new ApiOkResponse< List<Dictionary<string,object?>>>(list));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}