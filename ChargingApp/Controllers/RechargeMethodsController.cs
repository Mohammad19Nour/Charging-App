using ChargingApp.DTOs;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

public class RechargeMethodsController :BaseApiController
{
    private readonly IRechargeMethodeRepository _rechargeMethodeRepo;

    public RechargeMethodsController(IRechargeMethodeRepository rechargeMethodeRepo)
    {
        _rechargeMethodeRepo = rechargeMethodeRepo;
    }

    [HttpGet("recharge-methods-available")]
    public async Task<ActionResult<List<RechargeMethodDto>?>> GetAllRechargeMethods()
    {
        return Ok(new ApiOkResponse(result: await _rechargeMethodeRepo.GetRechargeMethodsAsync()));
    }
}