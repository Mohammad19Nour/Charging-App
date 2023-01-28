using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

public class AdminVipLevelController : AdminController
{
    private readonly IUnitOfWork _unitOfWork;

    public AdminVipLevelController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpPost("add-new-vip-level")]
    public async Task<ActionResult> AddNewVipLevel([FromBody] NewVipLevel dto)
    {
        try
        {
            var tmp = await _unitOfWork.VipLevelRepository.CheckIfExist(dto.VipLevel);

            if (tmp || dto.VipLevel == 0)
                return BadRequest(new ApiResponse(400, "already exist"));

            var vip = new VIPLevel
            {
                VipLevel = dto.VipLevel,
                MinimumPurchase = dto.MinimumPurchase,
                BenefitPercent = dto.BenefitPercent
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

    [HttpPatch("update-vip-level/{vipLevel:int}")]
    public async Task<ActionResult> UpdateMinPurchasing(int vipLevel, JsonPatchDocument patch)
    {
        try
        {
            var vip = await _unitOfWork.VipLevelRepository.GetVipLevelAsync(vipLevel);

            if (vip is null) return NotFound(new ApiResponse(404, "vip level not found"));

            var list = patch.Operations.Select(x => x.path.ToLower());
            var properties = typeof(VipLevelInfo).GetProperties();
            var op = patch.Operations.Select(x => x.op.ToLower());

            var tmp = op.FirstOrDefault(x => x != "replace");

            if (tmp != null) return BadRequest(new ApiResponse(400, "operation can be replace only"));

            var propertiesName = properties.Select(x => x.Name.ToLower()).ToList();

            if (vipLevel == 0)
            {
                propertiesName = propertiesName.FindAll(x => x == "benefitpercent");
            }

            foreach (var path in list)
            {
                if (propertiesName.FirstOrDefault(x => x == path) is null)
                    return BadRequest(new ApiResponse(400, path + " property isn't exist"));
            }

            patch.ApplyTo(vip);

            if (await _unitOfWork.Complete())
                return Ok(new ApiResponse(200, "updated successfully"));
            return BadRequest(new ApiResponse(400, "Failed to update product"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}