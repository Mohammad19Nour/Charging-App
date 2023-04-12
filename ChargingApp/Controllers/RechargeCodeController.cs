using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Extentions;
using ChargingApp.Helpers;
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

    [Authorize(Policy = "Required_JustVIP_Role")]
    [HttpPost]
    public async Task<ActionResult<decimal>> Recharge([FromBody] MyClass obj)
    {
        try
        {
            var email = User.GetEmail();
            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(email);

            if (user is null)
                return Unauthorized(new ApiResponse(404));

            var rols = User.GetRoles().ToList();
            if (SomeUsefulFunction.CheckIfItIsAnAdmin(rols))
                return BadRequest(new ApiResponse(403, "You're an admin... can't make an order with this account"));

            var tmpCode = await _unitOfWork.RechargeCodeRepository.GetCodeAsync(obj.Code);
           
            if (tmpCode is null || tmpCode.WasTaken)
                return BadRequest(new ApiResponse(401, "Invalid Code","الكود خاطئ... يرجى التأكد والمحاولة لاحقا"));

            tmpCode.WasTaken = true;
            tmpCode.User = user;
            user.Balance += tmpCode.Value;
            tmpCode.TakedTime = DateTime.Now;

            _unitOfWork.UserRepository.UpdateUserInfo(user);

            var curr = new NotificationHistory()
            {
                User = user,
                ArabicDetails = " تم شحن رصيدك بقيمة " + tmpCode.Value + " من خلال الكود: " + obj.Code,
                EnglishDetails = "Your balance has been charged with a value of " + tmpCode.Value +
                                 " using the code: " + obj.Code
            };
            _unitOfWork.NotificationRepository.AddNotificationForHistoryAsync(curr);

            if (await _unitOfWork.Complete())
                return Ok(new ApiOkResponse("Recharged successfully. your balance is " + user.Balance));

            return BadRequest(new ApiResponse(400, "something went wrong"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [Authorize(Policy = "Required_Admins_Role")]
    [HttpGet("generate-codes")]
    public async Task<ActionResult<IEnumerable<string>>> GetCodes(int codeValue, int codeNumber)
    {
        try
        {
            
            var codes = await _unitOfWork.RechargeCodeRepository.GenerateCodesWithValue(codeNumber, codeValue);

            if (codes is null)
                return BadRequest(new ApiResponse(400, "should provide number of codes and their values"));

            return Ok(new ApiOkResponse(codes));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public class MyClass
    {
        public string Code { get; set; }
    }
}