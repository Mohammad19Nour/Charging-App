using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

public class SupportNumberController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;

    public SupportNumberController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet("get-support-numbers")]
    [ProducesResponseType(typeof(ApiOkResponse<List<string>>), StatusCodes.Status200OK)]

    public async Task<ActionResult<List<string>>> GetSupportNumbers()
    {
        try
        {
            return Ok(new ApiOkResponse<List<string>>(await _unitOfWork.SupportNumberRepository
                .GetSupportNumbersAsync()));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}