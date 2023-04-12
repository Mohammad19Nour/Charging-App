using ChargingApp.DTOs;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

//[Authorize]
public class CurrencyController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;

    public CurrencyController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet("currencies")]
    [ProducesResponseType(typeof(ActionResult<List<CurrencyDto>>),StatusCodes.Status200OK)]

    public async Task<ActionResult<List<CurrencyDto>>> GetCurrencies()
    {
        try
        {
            var res = await _unitOfWork.CurrencyRepository.GetCurrencies();
            return Ok(new ApiOkResponse< List<CurrencyDto>>(res));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}