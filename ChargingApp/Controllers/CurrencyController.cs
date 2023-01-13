using ChargingApp.DTOs;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

public class CurrencyController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;

    public CurrencyController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet("currencies")]
    public async Task<ActionResult<List<CurrencyDto>>> GetCurrencies()
    {
       var res = await _unitOfWork.CurrencyRepository.GetCurrencies();
       return Ok(new ApiOkResponse(res));
    }
}