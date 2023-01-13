using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

public class AdminSliderController : AdminController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPhotoService _photoService;

    public AdminSliderController(IUnitOfWork unitOfWork, IPhotoService photoService)
    {
        _unitOfWork = unitOfWork;
        _photoService = photoService;
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult> AddPhoto([FromForm] IFormFile file)
    {
        var result = await _photoService.AddPhotoAsync(file);
        if (result.Error != null)
            return BadRequest(new ApiResponse(400, result.Error.Message));

        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };
        _unitOfWork.SliderRepository.AddPhoto(new SliderPhoto
        {
            Photo = photo,
            PhotoId = photo.Id
        });

        if (await _unitOfWork.Complete())
            return Ok(new ApiResponse(200, "Added successfully"));

        return BadRequest(new ApiResponse(400, "Failed to upload photo"));
    }

    [HttpPost("delete-photo")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var photo = await _unitOfWork.SliderRepository.GetPhotoNoTrackingByIdAsync(photoId);

        if (photo is null) return NotFound(new ApiResponse(404, "photo not found"));

        _unitOfWork.SliderRepository.DeletePhoto(photo);

        if (await _unitOfWork.Complete())
        {
            var res = await _photoService.DeletePhotoAsync(photo.Photo.PublicId);
            if (res.Error == null)
                return Ok(new ApiResponse(200, "Deleted successfully"));
        }

        return BadRequest(new ApiResponse(400, "some thing went wrong"));


        return BadRequest(new ApiResponse(400, "Failed to delete photo"));
    }
}