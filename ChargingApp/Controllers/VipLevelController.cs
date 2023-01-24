using AutoMapper;
using ChargingApp.DTOs;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;
[Authorize(Policy = "RequiredVIPRole")]
public class VipLevelController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public VipLevelController(IUnitOfWork unitOfWork , IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpGet("vip-levels")]
    public async Task<ActionResult<List<VipLevelDto>>> GetVipLevels()
    {
        var levels = await _unitOfWork.VipLevelRepository.GetAllVipLevelsAsync();
        levels = levels.Where(x => x.VipLevel != 0).ToList();
        
        var res = levels.Select(x=>_mapper.Map<VipLevelDto>(x));

        return Ok(new ApiOkResponse(res));
    }
}