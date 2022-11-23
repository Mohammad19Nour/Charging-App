using Charging_App.Errors;
using Microsoft.AspNetCore.Mvc;

namespace Charging_App.Controllers;

public class BuggyController : BaseApiController
{
    [HttpGet("not-found")]
    public ActionResult GetNotFoundRequest()
    {
        return NotFound(new ApiResponse(404));
    }
    
    [HttpGet("badrequest")]
    public ActionResult GetBadRequest()
    {
        return BadRequest();
    }
}