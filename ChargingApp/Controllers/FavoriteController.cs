using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Extentions;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

public class FavoriteController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;

    public FavoriteController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    [HttpGet("favorite-products")]
    public async Task<ActionResult<List<ProductDto>>> GetFavoriteProducts()
    {
        var email = User.GetEmail();
        if (email is null) return Unauthorized(new ApiResponse(401));

       
        var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(email);
        if (user is null) return Unauthorized(new ApiResponse(401));

        var res = await _unitOfWork.FavoriteRepository.GetFavoriteProductsForUserAsync(user.Id);

        var turkish = await _unitOfWork.CurrencyRepository.GetTurkishCurrency();
        var syrian = await _unitOfWork.CurrencyRepository.GetSyrianCurrency();
        foreach (var t in res)
        {
            t.TurkishPrice = t.Price * turkish;
            t.SyrianPrice = t.Price * syrian;
        }
        return Ok(new ApiOkResponse(res));
    }

    [HttpPost("{productId:int}")]
    public async Task<ActionResult> ToggleFavorite(int productId)
    {
        var email = User.GetEmail();
        if (email is null) return Unauthorized(new ApiResponse(403));

        var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(email);
        if (user is null) return Unauthorized(new ApiResponse(403));

        var product = await _unitOfWork.ProductRepository.GetProductByIdAsync(productId);

        if (product is null) return NotFound(new ApiResponse(404, "product not found"));

        var fav = new Favorite
        {
            User = user,
            Product = product,
            ProductId = product.Id,
            UserId = user.Id
        };
        var res = await _unitOfWork.FavoriteRepository.CheckIfExist(user.Id , product.Id);
        Console.WriteLine(res+"\n\n");

        if (!res)
            _unitOfWork.FavoriteRepository.AddFavoriteProduct(fav);
        else _unitOfWork.FavoriteRepository.DeleteFavoriteProduct(fav);

        if (await _unitOfWork.Complete()) return Ok(new ApiResponse(200));
        return BadRequest(new ApiResponse(400, "something went wrong"));
    }
}