using ChargingApp.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

public class TestT :BaseApiController
{
    
    [Authorize(Policy = "RequiredAdminRole")]
    [HttpGet]
    public async Task<ActionResult> Te()
    {
        return Ok(new ApiResponse(200));
    }
}