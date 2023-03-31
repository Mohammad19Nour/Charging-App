using System.Reflection;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

[Authorize(Policy = "Required_Administrator_Role")]
public class AdminBenefitController : AdminController
{
    private readonly IUnitOfWork _unitOfWork;

    public AdminBenefitController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpPost("add-specific-benefit-for-product")]
    public async Task<ActionResult> AddSpecificBenefitForProduct(int productId, int vipLevel, decimal benefitPercent)
    {
        try
        {
            var product = await _unitOfWork.ProductRepository.GetProductByIdAsync(productId);

            if (product is null)
                return NotFound(new ApiResponse(404, "product not found"));
            var vip = await _unitOfWork.VipLevelRepository.CheckIfExist(vipLevel);

            if (!vip)
                return NotFound(new ApiResponse(404, "vip level not found"));
            var spec = await _unitOfWork.BenefitPercentInSpecificVipLevelRepository
                .GetBenefitAsync(productId, vipLevel);

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
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }
}