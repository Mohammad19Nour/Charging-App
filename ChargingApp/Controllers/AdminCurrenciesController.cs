using ChargingApp.DTOs;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

public class AdminCurrenciesController : AdminController
{
    private readonly IUnitOfWork _unitOfWork;

    public AdminCurrenciesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    [HttpPost("update-currency")]
    public async Task<ActionResult> UpdateCurrency([FromBody] CurrencyDto dto)
    {
        dto.Name = dto.Name.ToLower();

        if (!await _unitOfWork.CurrencyRepository.CheckIfExistByNameAsync(dto.Name))
            return BadRequest(new ApiResponse(404, "currency not found"));
        
        _unitOfWork.CurrencyRepository.UpdateByNameAsync(dto.Name , dto.ValuePerDollar);

        if (await _unitOfWork.Complete())
            return Ok(new ApiResponse(200, "Updated successfully"));
        return BadRequest(new ApiResponse(400, "Failed to update currency"));
    }
}