using AutoMapper;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

public class AdminRechargeMethodeController : AdminController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AdminRechargeMethodeController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpPost("add-agent/{rechargeMethodId:int}")]
    public async Task<ActionResult> AddAgent(int rechargeMethodId, [FromBody] NewAgentDto dto)
    {
        try
        {
            if (rechargeMethodId == 1)
                return BadRequest(
                    new ApiResponse(403, "you can't add to this method "));

            var rechargeMethod = await _unitOfWork.RechargeMethodeRepository
                .GetRechargeMethodByIdAsync(rechargeMethodId);

            if (rechargeMethod is null)
                return NotFound(new ApiResponse(404, "recharge method not found"));

            var agent = _mapper.Map<ChangerAndCompany>(dto);
            _unitOfWork.RechargeMethodeRepository.AddAgent(rechargeMethod, agent);

            if (await _unitOfWork.Complete())
                return Ok(new ApiOkResponse(_mapper.Map<AgentDto>(agent)));

            return BadRequest(new ApiResponse(400, "Failed to add a new agent"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteAgent(int agentId, int rechargeMethodId)
    {
        try
        {
            var method = await _unitOfWork.RechargeMethodeRepository.GetRechargeMethodByIdAsync(rechargeMethodId);

            if (method is null)
                return BadRequest(new ApiResponse(403, "Recharge method not found"));

            var agent = method.ChangerAndCompanies.FirstOrDefault(x => x.Id == agentId);

            if (agent is null)
                return BadRequest(new ApiResponse(403, "Agent not found"));

            _unitOfWork.RechargeMethodeRepository.DeleteAgent(agent);

            if (await _unitOfWork.Complete())
                return Ok(new ApiResponse(200, "Deleted successfully"));

            return BadRequest(new ApiResponse(400, "Failed to delete new agent"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}