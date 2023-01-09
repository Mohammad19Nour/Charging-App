using AutoMapper;
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
}