using ChargingApp.Errors;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

[ApiController]
[Route("api/[controller]")]

[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiException), StatusCodes.Status500InternalServerError)]
public class BaseApiController : ControllerBase
{
}