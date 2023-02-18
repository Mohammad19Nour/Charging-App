using ChargingApp.DTOs;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

public class AdminCurrenciesController : AdminController
{
    private readonly IUnitOfWork _unitOfWork;

    public AdminCurrenciesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [Authorize(Policy = "Required_AllAdminExceptNormal_Role")]
    [HttpPost("update-currency")]
    public async Task<ActionResult> UpdateCurrency([FromBody] CurrencyDto dto)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.Name))
                return BadRequest(new ApiResponse(400, "currency name can't be empty"));

            dto.Name = dto.Name.ToLower();

            if (!await _unitOfWork.CurrencyRepository.CheckIfExistByNameAsync(dto.Name))
                return BadRequest(new ApiResponse(404, "currency not found"));

            var currency = await _unitOfWork.CurrencyRepository.GetCurrencyByNameAsync(dto.Name);

            if (currency is null)
                return BadRequest(new ApiResponse(400, "something went wrong during updating currency"));

            currency.ValuePerDollar = dto.ValuePerDollar;

            _unitOfWork.CurrencyRepository.UpdateByName(currency);

            if (await _unitOfWork.Complete())
                return Ok(new ApiResponse(200, "Updated successfully"));

            if (_unitOfWork.HasChanges())
                return BadRequest(new ApiResponse(400, "Failed to update currency"));
            return Ok(new ApiResponse(200));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}