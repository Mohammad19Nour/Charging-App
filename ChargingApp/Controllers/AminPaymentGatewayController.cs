using AutoMapper;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

public class AdminPaymentGatewayController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPhotoService _photoService;

    public AdminPaymentGatewayController(IUnitOfWork unitOfWork, IMapper mapper,IPhotoService photoService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _photoService = photoService;
    }

    [Authorize(Policy = "Required_Administrator_Role")]
    [HttpPut("update/{id:int}")]
    public async Task<ActionResult<PaymentGateway>> UpdateAddress(int id
        , [FromBody] UpdatePaymentGatewayDto dto)
    {
        var gateway = await _unitOfWork.PaymentGatewayRepository.GetPaymentGatewayByIdAsync(id);

        if (gateway == null)
            return BadRequest(new ApiResponse(404, "Gateway with id " + id + " not found"));
       
        _mapper.Map(dto, gateway);

        if (await _unitOfWork.Complete()) return Ok(new ApiOkResponse<PaymentGateway>(gateway));
        return BadRequest(new ApiResponse(400, "Failed to update"));
    }

    
    [Authorize(Policy = "Required_AnyAdmin_Role")]
    [HttpPut("update-photo/{id:int}")]
    public async Task<ActionResult<PaymentGatewayDto>> UpdatePhoto(int id
        , IFormFile imageFile)
    {
        var gateway = await _unitOfWork.PaymentGatewayRepository.GetPaymentGatewayByIdAsync(id);

        if (gateway == null)
            return BadRequest(new ApiResponse(404, "Gateway with id " + id + " not found"));

        if (imageFile == null) return BadRequest(new ApiResponse(400, "Image file is required"));


        var result = await _photoService.AddPhotoAsync(imageFile);

        if (!result.Success)
            return BadRequest(new ApiResponse(400, result.Message));

        gateway.Photo.Url = result.Url;
        
        if (await _unitOfWork.Complete()) return Ok(new ApiOkResponse <PaymentGatewayDto>(
            _mapper.Map<PaymentGatewayDto>(gateway)));
        
        return BadRequest(new ApiResponse(400, "Failed to update"));
    }
}