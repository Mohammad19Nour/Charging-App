using ChargingApp.Data;
using ChargingApp.DTOs;
using ChargingApp.Errors;
using ChargingApp.Extentions;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

//[Authorize]
public class RechargeCodeController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;

    public RechargeCodeController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [Authorize(Policy = "RequiredVIPRole")]
    [HttpPost]
    public async Task<ActionResult<double>> Recharge([FromBody] MyClass obj)
    {
        var email = User.GetEmail();
        var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(email);

        if (user is null)
            return Unauthorized(new ApiResponse(404));

        var tmpCode = await _unitOfWork.RechargeCodeRepository.GetCodeAsync(obj.Code);
        if (tmpCode is null || tmpCode.Istaked)
            return BadRequest(new ApiResponse(401, "Invalid Code"));

        tmpCode.Istaked = true;
        tmpCode.User = user;
        user.Balance += tmpCode.Value;
        tmpCode.TakedTime = DateTime.Now;

        if (await _unitOfWork.Complete())
            return Ok(new ApiOkResponse("Recharge successfully. your balance is " + user.Balance));

        return BadRequest(new ApiResponse(400, "something went wrong"));
    }


    //[Authorize(Policy = "RequiredAdminRole")]
    [HttpGet("generate-codes")]
    public async Task<ActionResult<IEnumerable<string>>> GetCodes(int codeValue, int codeNumber)
    {
        var codes = await _unitOfWork.RechargeCodeRepository.GenerateCodesWithValue(codeNumber, codeValue);

        if (codes is null)
            return BadRequest(new ApiResponse(400, "something happened"));

        return Ok(new ApiOkResponse(codes));
    }

    public class MyClass
    {
        public string Code { get; set; }
    }
}