using AutoMapper;
using ChargingApp.DTOs;
using ChargingApp.Errors;
using ChargingApp.Helpers;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

[Authorize(Policy = "Required_Admin2_Role")]
public class AdminReportController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AdminReportController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpGet("benefit")]
    public async Task<ActionResult> GetBenefit([FromQuery] DateQueryDto dto)
    {
        try
        {
            var (ans, msg) = SomeUsefulFunction.CheckDate(dto);

            if (!ans) return BadRequest(new ApiResponse(400, msg));

            var res = await CalcBenefit(dto);
            return Ok(new ApiOkResponse(res));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpGet("debits")]
    public async Task<ActionResult<List<DebitDto>>> GetDebits([FromQuery] DateQueryDto dto)
    {
        try
        {
            var (ans, msg) = SomeUsefulFunction.CheckDate(dto);

            if (!ans) return BadRequest(new ApiResponse(400, msg));

            var res = await _unitOfWork.DebitRepository.GetDebits(dto, null);
            return Ok(new ApiOkResponse(res));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }


    [HttpGet("sells")]
    public async Task<ActionResult> GetSells([FromQuery] DateQueryDto dto)
    {
        try
        {
            var (ans, msg) = SomeUsefulFunction.CheckDate(dto);

            if (!ans) return BadRequest(new ApiResponse(400, msg));

            var res = await _unitOfWork.OrdersRepository.GetDoneOrders(dto, null);
            res = res.Where(x => x.Status == 1).ToList();
            var listOfSells = res.Select(x => _mapper.Map<SellsDto>(x));
            var sellsDto = listOfSells as SellsDto[] ?? listOfSells.ToArray();
            var totalPurchasing = sellsDto.Sum(x => x.TotalPrice);

            return Ok(new ApiOkResponse(new { listOfSells = sellsDto, totalPurchasing }));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private async Task<decimal> CalcBenefit(DateQueryDto dto)
    {
        decimal res = 0;
        var doneOrders = await _unitOfWork.OrdersRepository.GetDoneOrders(dto, null);

        doneOrders = doneOrders.Where(x => x.Status == 1).ToList();
        foreach (var t in doneOrders)
        {
            if (!t.CanChooseQuantity)
                res += t.TotalPrice - t.Price * t.TotalQuantity;
            else
            {
                if (t.Price == t.TotalPrice)
                {
                    var perUnit = t.Price / t.Quantity;
                    var rem = t.Quantity - t.TotalQuantity;
                    res += rem * perUnit;
                }
                else
                {
                    res += t.TotalPrice - t.Price;
                }
            }
        }

        return res;
    }
}