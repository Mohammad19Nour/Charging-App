using AutoMapper;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

public class AdminPaymentGatewayController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AdminPaymentGatewayController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpPut("update/{id:int}")]
    public async Task<ActionResult<PaymentGateway>> UpdateAddress(int id
        , [FromBody] UpdatePaymentGatewayDto dto)
    {
        var gateway = await _unitOfWork.PaymentGatewayRepository.GetPaymentGatewayByIdAsync(id);

        if (gateway == null)
            return BadRequest(new ApiResponse(404, "Gateway with id " + id + " not found"));
        _mapper.Map(dto, gateway);

        if (await _unitOfWork.Complete()) return Ok(new ApiOkResponse(gateway));
        return BadRequest(new ApiResponse(400, "Failed to update"));
    }
}