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
}