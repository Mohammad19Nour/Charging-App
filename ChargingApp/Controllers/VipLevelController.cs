using AutoMapper;
using ChargingApp.DTOs;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ChargingApp.Controllers;

[Authorize(Policy = "Required_VIP_Role")]
public class VipLevelController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public VipLevelController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpGet("vip-levels")]
    [ProducesResponseType(typeof(ApiOkResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<VipLevelDto>>> GetVipLevels()
    {
        try
        {
            var levels = await _unitOfWork.VipLevelRepository.GetAllVipLevelsAsync();
            levels = levels.Where(x => x.VipLevel != 0).ToList();

            var res = levels.Select(x => _mapper.Map<VipLevelDto>(x));

            return Ok(new ApiOkResponse(res));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}