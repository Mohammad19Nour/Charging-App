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
    private readonly IPhotoService _photoService;

    public AdminVipLevelController(IUnitOfWork unitOfWork, IMapper mapper
        , IPhotoService photoService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _photoService = photoService;
    }

    [HttpGet("vip-levels")]
    public async Task<ActionResult<List<AdminVipLevelDto>>> GetVipLevels()
    {
        try
        {
            var levels = await _unitOfWork.VipLevelRepository.GetAllVipLevelsAsync();
          //  levels = levels.Where(x => x.VipLevel != 0).ToList();

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
    public async Task<ActionResult<AdminVipLevelDto>> AddNewVipLevel([FromForm] NewVipLevel dto)
    {
        try
        {
            var tmp = await _unitOfWork.VipLevelRepository.CheckIfExist(dto.VipLevel);

            if (tmp || dto.VipLevel == 0)
                return BadRequest(new ApiResponse(400, "Vip level already exist"));

            var vips = await _unitOfWork.VipLevelRepository.GetAllVipLevelsAsync();
            vips.Sort((x, y) => x.Id.CompareTo(y.Id));

            if (vips.Count > 1)
            {
                vips.Sort((x, y) => x.VipLevel.CompareTo(y.VipLevel));
                var last = vips.Last().VipLevel;

                if (dto.VipLevel != 1 + last)
                    return BadRequest(new ApiResponse(400, "New vip level must be " + (last + 1) ));
            }

            else if (dto.VipLevel != 1)
                return BadRequest(new ApiResponse(400, "New vip level must be " + 1));

            if (dto.ImageFile == null)
                return BadRequest(new ApiResponse(400, "Image file is required"));

            var result = await _photoService.AddPhotoAsync(dto.ImageFile);

            if (!result.Success)
                return BadRequest(new ApiResponse(400, "Failed to upload photo  " + result.Message));

            decimal prevSum = 0;

            if (vips.Count > 0)
                prevSum = vips.Last(x => x.VipLevel >= 0).MinimumPurchase +
                          vips.Last(x => x.VipLevel >= 0).Purchase;
            var vip = new VIPLevel
            {
                VipLevel = dto.VipLevel,
                Purchase = dto.Purchase,
                BenefitPercent = dto.BenefitPercent,
                MinimumPurchase = prevSum,
                ArabicName = dto.ArabicName,
                EnglishName = dto.EnglishName,
                Photo = new Photo { Url = result.Url }
            };

            _unitOfWork.VipLevelRepository.AddVipLevel(vip);

            if (await _unitOfWork.Complete())
                return Ok(new ApiOkResponse(_mapper.Map<AdminVipLevelDto>(vip)));

            return BadRequest(new ApiResponse(400, "something went wrong"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpPut("update-vip-level/{vipLevel:int}")]
    public async Task<ActionResult> UpdateMinPurchasing(int vipLevel, UpdateVipLevelDto dto)
    {
        try
        {
            var vip = await _unitOfWork.VipLevelRepository.CheckIfExist(vipLevel);

            if (!vip) return NotFound(new ApiResponse(404, "vip level not found"));

            var levels = await _unitOfWork.VipLevelRepository.GetAllVipLevelsAsync();

            
            if (dto.BenefitPercent != null)
                levels[vipLevel].BenefitPercent = (decimal)dto.BenefitPercent;

            if (dto.Purchase != null && vipLevel != 0)
                levels[vipLevel].Purchase = (decimal)dto.Purchase;

            if (!string.IsNullOrEmpty(dto.ArabicName))
                levels[vipLevel].ArabicName = dto.ArabicName;
            
            if (!string.IsNullOrEmpty(dto.EnglishName))
                levels[vipLevel].EnglishName = dto.EnglishName;

            for (int j = 1; j < levels.Count; j++)
            {
                levels[j].MinimumPurchase = levels[j - 1].MinimumPurchase + levels[j - 1].Purchase;
                _unitOfWork.VipLevelRepository.UpdateVipLevel(levels[j]);
            }
            
            if (vipLevel == 0) _unitOfWork.VipLevelRepository.UpdateVipLevel(levels[0]);

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

    [HttpPut("update-photo/{vipLevelId:int}")]
    public async Task<ActionResult> UpdateImage(int vipLevelId, IFormFile? imageFile)
    {
        var vipLevel = await _unitOfWork.VipLevelRepository.GetVipLevelAsync(vipLevelId);

        if (vipLevel == null)
            return BadRequest(new ApiResponse(400, "Vip level " + vipLevelId
                                                                + " not found"));
        if (imageFile == null) return BadRequest(new ApiResponse(400, "Image file is required"));

        var result = await _photoService.AddPhotoAsync(imageFile);

        if (!result.Success)
            return BadRequest(new ApiResponse(400, result.Message));

        var name = vipLevel.Photo.Url;
        vipLevel.Photo.Url = result.Url;

        _unitOfWork.VipLevelRepository.UpdateVipLevel(vipLevel);

        if (!await _unitOfWork.Complete())
            return BadRequest(new ApiResponse(400, "Failed to update the image... try again"));

        if (name != null) await _photoService.DeletePhotoAsync(name);

        return Ok(new ApiResponse(200, "Image updated successfully."));
    }
}