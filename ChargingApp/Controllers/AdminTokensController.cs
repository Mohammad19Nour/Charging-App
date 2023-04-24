using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

[Authorize(Policy = "Required_Admins_Role")]
public class AdminTokensController : AdminController
{
    private readonly IUnitOfWork _unitOfWork;

    public AdminTokensController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpPost("update-token")]
    public async Task<ActionResult> UpdateToken([FromQuery] string siteName,
        [FromBody] Tok param)
    {
        if (string.IsNullOrEmpty(siteName) || string.IsNullOrEmpty(param.Token))
            return BadRequest(new ApiResponse(400, "Should specify site name and its token"));

        siteName = siteName.ToLower();

        var hostingSite = await _unitOfWork.OtherApiRepository.GetHostingSiteByNameAsync(siteName);

        if (hostingSite is null)
            return BadRequest(new ApiResponse(400, siteName + " not found"));

        hostingSite.Token = param.Token;

        _unitOfWork.OtherApiRepository.UpdateHostingSite(hostingSite);

        if (await _unitOfWork.Complete())
            return Ok(new ApiResponse(200, "Token updated successfully"));

        return BadRequest(new ApiResponse(400, "Failed to update token"));
    }
}

public class Tok
{
    public string Token { get; set; }
}