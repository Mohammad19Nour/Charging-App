using ChargingApp.Errors;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

[Route("errors/{code}")]
[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorConrtoller :BaseApiController
{
    public IActionResult Error(int code)
    {
        return new ObjectResult(new ApiResponse(code));
    }
    
}