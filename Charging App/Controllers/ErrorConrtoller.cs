using Charging_App.Errors;
using Microsoft.AspNetCore.Mvc;

namespace Charging_App.Controllers;

[Route("errors/{code}")]
[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorConrtoller :BaseApiController
{
    public IActionResult Error(int code)
    {
        return new ObjectResult(new ApiResponse(code));
    }
    
}