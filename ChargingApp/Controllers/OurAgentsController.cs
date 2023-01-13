﻿using AutoMapper;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

public class OurAgentsController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public OurAgentsController(IUnitOfWork unitOfWork,IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpGet("our-agents")]
    public async Task<ActionResult<List<OurAgentsDto>>> GetOurAgents()
    {
        return Ok(new ApiOkResponse(await _unitOfWork.OurAgentsRepository.GetOurAgentsAsync()));
    }

    [HttpPost("add-agent")]
    public async Task<ActionResult<OurAgentsDto>> AddAgent([FromBody] NewOurAgentDto dto)
    {
       var agent = _mapper.Map<OurAgent>(dto);
        _unitOfWork.OurAgentsRepository.AddAgent(agent);

        if (await _unitOfWork.Complete()) return Ok(new ApiResponse(201, "agent added"));

        return BadRequest(new ApiResponse(400, "Failed to add agent"));
    }

    [HttpPut("update-agent")]
    public async Task<ActionResult> UpdateAgent([FromBody] UpdateOurAgentDto dto)
    {
        var agent = await _unitOfWork.OurAgentsRepository.GetAgentById(dto.Id);
        
       if (agent == null)
           return BadRequest(new ApiResponse(404, "agent not found"));
       Console.WriteLine(dto.City + "  " + agent.City+"\n\n\n\n");

       _mapper.Map(dto, agent);
       
       Console.WriteLine(dto.City + "  " + agent.City+"\n\n\n\n");
      _unitOfWork.OurAgentsRepository.UpdateAgent(agent);
       
       if (await _unitOfWork.Complete())
           return Ok(new ApiResponse(200, "Updated successfully"));
       
       return BadRequest(new ApiResponse(400, "Failed to update agent"));
    }

    [HttpDelete("{agentId:int}")]
    public async Task<IActionResult> DeleteOurAgent(int agentId)
    {
        var agent = await _unitOfWork.OurAgentsRepository.GetAgentById(agentId);
        
        if (agent == null) return BadRequest(new ApiResponse(404, "agent not found"));

         _unitOfWork.OurAgentsRepository.DeleteAgent(agent);
         
         if (await _unitOfWork.Complete())
            return Ok(new ApiResponse(200, "Deleted successfully"));
         
         return BadRequest(new ApiResponse(400, "Failed to delete agent"));
    }
}