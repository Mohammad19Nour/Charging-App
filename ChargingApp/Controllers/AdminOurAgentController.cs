using AutoMapper;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

[Authorize(Policy = "Required_Admins_Role")]
public class AdminOurAgentController : AdminController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AdminOurAgentController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpPost("add-agent")]
    public async Task<ActionResult<OurAgentsDto>> AddAgent([FromBody] NewOurAgentDto dto)
    {
        try
        {
            var agent = new OurAgent
            {
                ArabicName = dto.ArabicName,
                EnglishName = dto.EnglishName,
                City = dto.City,
                PhoneNumber = dto.PhoneNumber
            };
            _unitOfWork.OurAgentsRepository.AddAgent(agent);

            if (await _unitOfWork.Complete()) return Ok(new ApiResponse(201, "agent added"));

            return BadRequest(new ApiResponse(400, "Failed to add agent"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpPut("update-agent")]
    public async Task<ActionResult> UpdateAgent([FromBody] UpdateOurAgentDto dto)
    {
        try
        {
            var agent = await _unitOfWork.OurAgentsRepository.GetAgentById(dto.Id);

            if (agent == null)
                return BadRequest(new ApiResponse(404, "agent not found"));
            Console.WriteLine(dto.City + "  " + agent.City + "\n\n\n\n");

            _mapper.Map(dto, agent);

            Console.WriteLine(dto.City + "  " + agent.City + "\n\n\n\n");
            _unitOfWork.OurAgentsRepository.UpdateAgent(agent);

            if (await _unitOfWork.Complete())
                return Ok(new ApiResponse(200, "Updated successfully"));

            return BadRequest(new ApiResponse(400, "Failed to update agent"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpDelete("{agentId:int}")]
    public async Task<IActionResult> DeleteOurAgent(int agentId)
    {
        try
        {
            var agent = await _unitOfWork.OurAgentsRepository.GetAgentById(agentId);

            if (agent == null) return BadRequest(new ApiResponse(404, "agent not found"));

            _unitOfWork.OurAgentsRepository.DeleteAgent(agent);

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