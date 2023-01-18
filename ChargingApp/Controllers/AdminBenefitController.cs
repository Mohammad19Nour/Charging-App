using System.Reflection;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

public class AdminBenefitController : AdminController
{
    private readonly IUnitOfWork _unitOfWork;

    public AdminBenefitController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpPost("add-specific-benefit-for-product")]
    public async Task<ActionResult> AddSpecificBenefitForProduct(int productId , int vipLevel , double benefitPercent)
    {
        var product = await _unitOfWork.ProductRepository.GetProductByIdAsync(productId);

        if (product is null)
            return NotFound(new ApiResponse(404, "product not found"));
        var vip = await _unitOfWork.VipLevelRepository.CheckIfExist(vipLevel);
        
        if (!vip)
            return NotFound(new ApiResponse(404, "vip level not found"));
        var spec = await _unitOfWork.BenefitPercentInSpecificVipLevelRepository.GetBenefitAsync(productId, vipLevel);

        if (spec is null)
        {
            _unitOfWork.BenefitPercentInSpecificVipLevelRepository.AddBenefitPercentForProduct(
                new BenefitPercentInSpecificVilLevel
                {
                    ProductId = productId,
                    VipLevel = vipLevel,
                    BenefitPercent = benefitPercent
                });
        }
        else
            spec.BenefitPercent = benefitPercent;

        if (await _unitOfWork.Complete())
            return Ok(new ApiResponse(200));
        
        return BadRequest(new ApiResponse(400, "something went wrong"));

    }
    [HttpPost("update-minimumpurchasing/{vipLevel:int}")]
    public async Task<ActionResult> UpdateMinPurchasing(int vipLevel , JsonPatchDocument patch)
    {
        var vip = await _unitOfWork.VipLevelRepository.GetVipLevelAsync(vipLevel);

        if (vip is null) return NotFound(new ApiResponse(404, "vip level not found"));
       
        var list = patch.Operations.Select(x => x.path.ToLower());
        var properties = typeof(VipLevelInfo).GetProperties();
       var op = patch.Operations.Select(x => x.op.ToLower());

      var tmp= op.FirstOrDefault(x => x != "replace");

      if (tmp != null) return BadRequest(new ApiResponse(400,"operation can be replace only"));
        
        var propertiesName = properties.Select(x => x.Name.ToLower()).ToList();

        if (vipLevel == 0)
        {
            propertiesName = propertiesName.FindAll(x=>x == "benefitpercent");
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
}