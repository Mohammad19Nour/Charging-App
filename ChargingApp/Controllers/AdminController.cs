using ChargingApp.Extentions;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;
[Authorize (Policy = "RequiredAdminRole")]
public class AdminController :BaseApiController
{
  private readonly ICategoryRepository _repo;
  private readonly IPhotoService _photoService;

  public AdminController(ICategoryRepository repo , IPhotoService photoService)
  {
    _repo = repo;
    _photoService = photoService;
  }

  [HttpPost("add-photo")]
  public async Task<ActionResult> AddPhotoToCategory(IFormFile file ,int categoryId)
  {
    var email = User.GetEmail();
    var category = await _repo.GetCategoryByIdAsync(categoryId);

    if (category == null) return NotFound(new ApiResponse(400 , "This category isn't exist"));
    if (category.Photo != null)
    {
      var res = await _photoService.DeletePhotoAsync(category.Photo.PublicId);
      if (res.Error != null) return BadRequest(new ApiResponse(400 , res.Error.Message));
    }

    var result = await _photoService.AddPhotoAsync(file);
    var photo = new Photo
    {
      Url = result.SecureUrl.AbsoluteUri,
      PublicId = result.PublicId
    };

    category.Photo = photo;

    if (await _repo.SaveAllAsync()) return Ok(new ApiResponse(200 , "The photo is updated successfully"));

    return BadRequest(new ApiResponse(400 , "Failed to update photo to category"));
  }

  [HttpPut("update-category-name")]
  public async Task<ActionResult> UpdateCategoryName(int categoryId, string? arabicName, string? englishName)
  {
    var category = await _repo.GetCategoryByIdAsync(categoryId);
    
    if ( category == null) return NotFound(new ApiResponse(404, "Category not found"));
    if (arabicName == null && englishName == null)
      return BadRequest(new ApiResponse(400 , "At least one name should be not null"));

    if (arabicName != null)
      category.ArabicName = arabicName;

    if (englishName != null)
      category.EnglishName = englishName;
    
    if (await _repo.SaveAllAsync()) return Ok("Update successfully");

    return BadRequest(new ApiResponse(400,"Failed to update name"));
  }

  [HttpDelete]
  public async Task<ActionResult> DeleteCategory(int categoryId)
  {
    var result = await _repo.DeleteCategoryByIdAsync(categoryId);
    if (result) return Ok("Deleted Successfully");
    return BadRequest(new ApiResponse(400, "Failed to delete category"));
  }
}