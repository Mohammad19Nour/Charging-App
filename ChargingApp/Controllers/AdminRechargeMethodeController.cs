using AutoMapper;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
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


    [Authorize(Policy = "Required_AllAdminExceptNormal_Role")]
    [HttpPost("add-agent/{rechargeMethodId:int}")]
    public async Task<ActionResult> AddAgent(int rechargeMethodId, [FromBody] NewAgentDto dto)
    {
        try
        {
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

    [Authorize(Policy = "Required_Administrator_Role")]
    [HttpPut("update-agent/{agentId:int}")]
    public async Task<ActionResult> UpdateAgent(int agentId, int rechargeMethodId, [FromQuery] string? arabicName
        , [FromQuery] string? englishName)
    {
        try
        {
            var method = await _unitOfWork.RechargeMethodeRepository.GetRechargeMethodByIdAsync(rechargeMethodId);

            if (method is null)
                return BadRequest(new ApiResponse(400, "Recharge method not found"));

            var agent = method.ChangerAndCompanies.FirstOrDefault(x => x.Id == agentId);

            if (agent is null)
                return BadRequest(new ApiResponse(400, "Agent not found"));
            if (string.IsNullOrEmpty(arabicName) && string.IsNullOrEmpty(englishName)) 
                return BadRequest(new ApiResponse(400, 
                    "You should provide some information to be updated "));
           
            if (!string.IsNullOrEmpty(arabicName)) agent.ArabicName = arabicName;
            if (!string.IsNullOrEmpty(englishName)) agent.EnglishName = englishName;

            _unitOfWork.RechargeMethodeRepository.UpdateAgent(agent);
            if (await _unitOfWork.Complete())
            {
                return Ok(new ApiOkResponse(_mapper.Map<AgentDto>(agent)));
            }

            return BadRequest(new ApiResponse(400, "Failed to Update agent"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [Authorize(Policy = "Required_Administrator_Role")]
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

            return BadRequest(new ApiResponse(400, "Failed to delete agent"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}