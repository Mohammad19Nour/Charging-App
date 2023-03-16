using AutoMapper;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

[Authorize(Policy = "Required_Administrator_Role")]
public class AdminVipLevelController : AdminController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AdminVipLevelController(IUnitOfWork unitOfWork , IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpGet("vip-levels")]
    public async Task<ActionResult<List<AdminVipLevelDto>>> GetVipLevels()
    {
        try
        {
            var levels = await _unitOfWork.VipLevelRepository.GetAllVipLevelsAsync();
            levels = levels.Where(x => x.VipLevel != 0).ToList();

            var res = levels.Select(x => _mapper.Map<AdminVipLevelDto>(x));

            return Ok(new ApiOkResponse(res));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpPost("add-new-vip-level")]
    public async Task<ActionResult> AddNewVipLevel([FromBody] NewVipLevel dto)
    {
        try
        {
            var tmp = await _unitOfWork.VipLevelRepository.CheckIfExist(dto.VipLevel);

            if (tmp || dto.VipLevel == 0)
                return BadRequest(new ApiResponse(400, "Vip level already exist"));

            var vips = await _unitOfWork.VipLevelRepository.GetAllVipLevelsAsync();
            vips.Sort((x , y) =>x.Id.CompareTo(y.Id));

            if (vips.Count > 1)
            {
                vips.Sort((x, y) => x.VipLevel.CompareTo(y.VipLevel));
                var last = vips.Last().VipLevel;

                if (dto.VipLevel != 1 + last)
                    return BadRequest(new ApiResponse(400, "vip level should be " + last + 1));
            }

            else if (dto.VipLevel != 1)
                return BadRequest(new ApiResponse(400, "vip level should be " + 1));

            double prevSum = 0;

            if (vips.Count > 0)
                prevSum = vips.Last(x => x.VipLevel > 0).MinimumPurchase +
                          vips.Last(x => x.VipLevel > 0).Purchase ;
            var vip = new VIPLevel
            {
                VipLevel = dto.VipLevel,
                Purchase = dto.MinimumPurchase,
                BenefitPercent = dto.BenefitPercent,
                MinimumPurchase = prevSum
            };
            _unitOfWork.VipLevelRepository.AddVipLevel(vip);

            if (await _unitOfWork.Complete())
                return Ok(new ApiResponse(200));
            return BadRequest(new ApiResponse(400, "something went wrong"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpPut("update-vip-level/{vipLevel:int}")]
    public async Task<ActionResult> UpdateMinPurchasing(int vipLevel, VipLevelInfo dto)
    {
        try
        {
            var vip = await _unitOfWork.VipLevelRepository.CheckIfExist(vipLevel);

            if (!vip) return NotFound(new ApiResponse(404, "vip level not found"));

            var levels = await _unitOfWork.VipLevelRepository.GetAllVipLevelsAsync();

          
            if (dto.BenefitPercent != null)
                levels[vipLevel].BenefitPercent = (double)dto.BenefitPercent;

            if (dto.MinimumPurchase != null && vipLevel != 0)
                levels[vipLevel].Purchase = (double)dto.MinimumPurchase;

            for (int j = 1; j < levels.Count; j++)
            {
                levels[j].MinimumPurchase = levels[j-1].MinimumPurchase+levels[j-1].Purchase;
                _unitOfWork.VipLevelRepository.UpdateVipLevel(levels[j]);
            }

            if (await _unitOfWork.Complete())
                return Ok(new ApiResponse(200, "updated successfully"));
            return BadRequest(new ApiResponse(400, "Failed to update vip level"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}