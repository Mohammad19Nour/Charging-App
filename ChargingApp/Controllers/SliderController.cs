﻿using ChargingApp.DTOs;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

public class SliderController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;

    public SliderController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet("slider-photos")]
    [ProducesResponseType(typeof(ApiOkResponse<List<SliderPhotoDto>>), StatusCodes.Status200OK)]

    public async Task<ActionResult<List<SliderPhotoDto?>>> GetSliderPhotos()
    {
        try
        {
            var res = await _unitOfWork.SliderRepository.GetSliderPhotosAsync();
            return Ok(new ApiOkResponse<List<SliderPhotoDto>>(res));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}