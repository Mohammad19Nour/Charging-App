using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Extentions;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

[Authorize]
public class FavoriteController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;

    public FavoriteController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet("favorite")]
    public async Task<ActionResult<List<CategoryDto>>> GetFavoriteProducts()
    {
        var email = User.GetEmail();
        if (email is null) return Unauthorized(new ApiResponse(401));


        var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(email);
        if (user is null) return Unauthorized(new ApiResponse(401));

        var res = await _unitOfWork.FavoriteRepository.GetFavoriteCategoriesForUserAsync(user.Id);

        return Ok(new ApiOkResponse(res));
    }

    [HttpPost("{categoryId:int}")]
    public async Task<ActionResult> ToggleFavorite(int categoryId)
    {
        var email = User.GetEmail();
        if (email is null) return Unauthorized(new ApiResponse(403));

        var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(email);
        if (user is null) return Unauthorized(new ApiResponse(403));

        var category = await _unitOfWork.CategoryRepository.GetCategoryByIdAsync(categoryId);

        if (category is null) return NotFound(new ApiResponse(404, "category not found"));

        var fav = new Favorite
        {
            User = user,
            UserId = user.Id,
            Category = category,
            CategoryId = category.Id
        };
        var res = await _unitOfWork.FavoriteRepository.CheckIfExist(user.Id, category.Id);

        if (!res)
            _unitOfWork.FavoriteRepository.AddFavoriteCategory(fav);
        else _unitOfWork.FavoriteRepository.DeleteFavoriteCategory(fav);

        if (await _unitOfWork.Complete()) return Ok(new ApiResponse(200));
        return BadRequest(new ApiResponse(400, "something went wrong"));
    }
}