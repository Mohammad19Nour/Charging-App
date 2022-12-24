using ChargingApp.Data;
using ChargingApp.Errors;
using ChargingApp.Extentions;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

[Authorize]
public class RechargeCodeController : BaseApiController
{
    private readonly IRechargeCodeRepository _rechargeCodeRepo;
    private readonly DataContext _context;
    private readonly IUserRepository _userRpo;

    public RechargeCodeController(IRechargeCodeRepository rechargeCodeRepo, DataContext context
        , IUserRepository userRpo)
    {
        _rechargeCodeRepo = rechargeCodeRepo;
        _context = context;
        _userRpo = userRpo;
    }

    [HttpPost]
    public async Task<ActionResult> Recharge([FromBody] MyClass obj)
    {
        var email = User.GetEmail();
        var user = await _userRpo.GetUserByEmailAsync(email);

        if (user is null)
            return Unauthorized(new ApiResponse(404));

        if (user.VIPLevel == 0)
            return Unauthorized(new ApiResponse(403, "you can't do this action"));

        var tmpCode = await _rechargeCodeRepo.GetCodeAsync(obj.Code);
        if (tmpCode is null || tmpCode.Istaked)
            return BadRequest(new ApiResponse(401, "Invalid Code"));

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            tmpCode.Istaked = true;
            tmpCode.User = user;
            user.Balance += tmpCode.Value;
            tmpCode.TakedTime = DateTime.Now;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return Ok(new ApiResponse(201, "Recharged successfully. your balance is: "+user.Balance));
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            return BadRequest(new ApiResponse(400, "something went wrong"));
        }
    }
 
    
    [HttpGet]
  //  [Authorize (Policy = "RequiredAdminRole")]
    public async Task<ActionResult<IEnumerable<string>>> GetCodes(int codeValue, int codeNumber)
    {
        var codes = await _rechargeCodeRepo.GenerateCodesWithValue(codeNumber, codeValue);

        if (codes is null)
            return BadRequest(new ApiResponse(400, "something happened"));

        return Ok(new ApiOkResponse(codes));
    }
  public class MyClass
  {
      public string Code { get; set; }
  }
}