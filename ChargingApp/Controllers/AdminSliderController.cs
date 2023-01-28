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
        try
        {
            var result = await _photoService.AddPhotoAsync(file);

            if (!result.Success)
                return BadRequest(new ApiResponse(400, result.Message));

            var photo = new Photo
            {
                Url = result.Url
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
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpDelete("delete-photo")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        try
        {
            var photo = await _unitOfWork.SliderRepository.GetPhotoNoTrackingByIdAsync(photoId);

            if (photo is null) return NotFound(new ApiResponse(404, "photo not found"));

            _unitOfWork.SliderRepository.DeletePhoto(photo);

            if (!await _unitOfWork.Complete()) return BadRequest(new ApiResponse(400, "some thing went wrong"));
            var res = await _photoService.DeletePhotoAsync(photo.Photo.Url);
            if (res)
                return Ok(new ApiResponse(200, "Deleted successfully"));

            return BadRequest(new ApiResponse(400, "some thing went wrong"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}