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
    private readonly IPhotoService _photoService;

    public AdminRechargeMethodeController(IUnitOfWork unitOfWork, IMapper mapper
        , IPhotoService photoService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _photoService = photoService;
    }

    [Authorize(Policy = "Required_AllAdminExceptNormal_Role")]
    [HttpPost("add-agent/{rechargeMethodId:int}")]
    public async Task<ActionResult<AgentDto>> AddAgent(int rechargeMethodId, [FromForm] NewAgentDto dto)
    {
        try
        {
            var rechargeMethod = await _unitOfWork.RechargeMethodeRepository
                .GetRechargeMethodByIdAsync(rechargeMethodId);

            if (rechargeMethod is null)
                return NotFound(new ApiResponse(404, "recharge method not found"));

            if (dto.ImageFile == null) return BadRequest(new ApiResponse(400, "image file is required"));


            var result = await _photoService.AddPhotoAsync(dto.ImageFile);

            if (!result.Success)
                return BadRequest(new ApiResponse(400, result.Message));

            var agent = new ChangerAndCompany
            {
                ArabicName = dto.ArabicName,
                EnglishName = dto.EnglishName,
                RechargeMethodMethod = rechargeMethod,
                Photo = new Photo { Url = result.Url }
            };
            _unitOfWork.RechargeMethodeRepository.AddAgent(rechargeMethod, agent);


            if (await _unitOfWork.Complete())
                return Ok(new ApiOkResponse<AgentDto>(_mapper.Map<AgentDto>(agent)));

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
    public async Task<ActionResult<AgentDto>> UpdateAgent(int agentId, int rechargeMethodId,
        [FromQuery] string? arabicName
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
                return Ok(new ApiOkResponse<AgentDto>(_mapper.Map<AgentDto>(agent)));
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
    [HttpPut("update-Method-photo/{rechargeMethodId:int}")]
    public async Task<ActionResult<ApiResponse>> UpdateMethodPhoto(int rechargeMethodId, IFormFile? imageFile)
    {
        var method = await _unitOfWork.RechargeMethodeRepository.GetRechargeMethodByIdAsync(rechargeMethodId);

        if (method is null)
            return BadRequest(new ApiResponse(400, "Recharge method not found"));

        if (imageFile == null) return BadRequest(new ApiResponse(400, "image file is required"));


        var result = await _photoService.AddPhotoAsync(imageFile);

        if (!result.Success)
            return BadRequest(new ApiResponse(400, result.Message));

        method.Photo.Url = result.Url;
        _unitOfWork.RechargeMethodeRepository.Update(method);

        if (await _unitOfWork.Complete())
        {
            return Ok(new ApiResponse(200, "Image updated successfully."));
        }

        return BadRequest(new ApiResponse(400, "Failed to update the image... try again"));
    }

    [Authorize(Policy = "Required_AllAdminExceptNormal_Role")]
    [HttpPut("update-agent-photo/{rechargeMethodId:int}")]
    public async Task<ActionResult<ApiResponse>> UpdateAgentPhoto(int rechargeMethodId, int agentId,
        IFormFile? imageFile)
    {
        var method = await _unitOfWork.RechargeMethodeRepository.GetRechargeMethodByIdAsync(rechargeMethodId);

        if (method is null)
            return BadRequest(new ApiResponse(400, "Recharge method not found"));

        var agent = method.ChangerAndCompanies.FirstOrDefault(x => x.Id == agentId);

        if (agent is null)
            return BadRequest(new ApiResponse(400, "Agent not found"));

        if (imageFile == null) return BadRequest(new ApiResponse(400, "image file is required"));


        var result = await _photoService.AddPhotoAsync(imageFile);

        if (!result.Success)
            return BadRequest(new ApiResponse(400, result.Message));

        agent.Photo.Url = result.Url;
        _unitOfWork.RechargeMethodeRepository.UpdateAgent(agent);

        if (await _unitOfWork.Complete())
        {
            return Ok(new ApiResponse(200, "Image updated successfully."));
        }

        return BadRequest(new ApiResponse(400, "Failed to update the image... try again"));
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