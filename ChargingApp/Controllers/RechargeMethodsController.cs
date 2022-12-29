using AutoMapper;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

public class RechargeMethodsController : BaseApiController
{
    private readonly IRechargeMethodeRepository _rechargeMethodeRepo;
    private readonly IMapper _mapper;

    public RechargeMethodsController(IRechargeMethodeRepository rechargeMethodeRepo,
        IMapper mapper)
    {
        _rechargeMethodeRepo = rechargeMethodeRepo;
        _mapper = mapper;
    }

    [HttpGet("recharge-methods-available")]
    public async Task<ActionResult<List<RechargeMethodDto>?>> GetAllRechargeMethods()
    {
        return Ok(new ApiOkResponse(result: await _rechargeMethodeRepo.GetRechargeMethodsAsync()));
    }

    [HttpPost("add-agent/{rechargeMethodId:int}")]
    public async Task<ActionResult> AddAgent(int rechargeMethodId, [FromBody] NewAgentDto dto)
    {   
        if (rechargeMethodId == 1)
            return BadRequest(
                new ApiResponse(403, "you can't add to this method "));
        var rechargeMethod = await _rechargeMethodeRepo
            .GetRechargeMethodByIdAsync(rechargeMethodId);
        if (rechargeMethod is null)
            return NotFound(new ApiResponse(404, "recharge method not found"));

        var agent = _mapper.Map<ChangerAndCompany>(dto);
        _rechargeMethodeRepo.AddAgent(rechargeMethod, agent);

        if (await _rechargeMethodeRepo.SaveAllChangesAsync())
            return Ok(new ApiOkResponse(_mapper.Map<AgentDto>(agent)));

        return BadRequest(new ApiResponse(400, "Failed to add a new agent"));
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteAgent(int agentId, int rechargeMethodId)
    {
        var method = await _rechargeMethodeRepo.GetRechargeMethodByIdAsync(rechargeMethodId);

        //Console.WriteLine(method.ChangerAndCompanies.Count+"\n\n");
        if (method is null)
            return BadRequest(new ApiResponse(403, "Recharge method not found"));
        if ((method.ChangerAndCompanies is null) ||
            (method.ChangerAndCompanies.FirstOrDefault(x => x.Id == agentId) is null))
            return BadRequest(new ApiResponse(403, "Agent not found"));

        if (await _rechargeMethodeRepo.DeleteAgent(agentId))
            return Ok(new ApiResponse(200, "Deleted successfully"));

        return BadRequest(new ApiResponse(400, "Failed to delete new agent"));
    }
}