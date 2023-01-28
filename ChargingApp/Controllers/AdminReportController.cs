using AutoMapper;
using ChargingApp.DTOs;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

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
            var (ans, msg) = CheckDate(dto);

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
            var (ans, msg) = CheckDate(dto);

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
            var (ans, msg) = CheckDate(dto);

            if (!ans) return BadRequest(new ApiResponse(400, msg));

            var res = await _unitOfWork.OrdersRepository.GetDoneOrders(dto, null);
            var listOfSells = res.Select(x => _mapper.Map<SellsDto>(x));

            var sellsDto = listOfSells as SellsDto[] ?? listOfSells.ToArray();
            var totalPurchasing = sellsDto.Sum(x => x.TotalPrice);
        
            return Ok(new ApiOkResponse(new { listOfSells = sellsDto , totalPurchasing}));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private (bool Res, string Message) CheckDate(DateQueryDto dto)
    {
        if (dto.Year == null)
            return (false, "you should specify year");

        if (dto.Month is null && dto.Day != null)
            return (false, "you should specify month of day");

        return (true, "");
    }

    private async Task<double> CalcBenefit(DateQueryDto dto)
    {
        var res = 0.0;
        var doneOrders = await _unitOfWork.OrdersRepository.GetDoneOrders(dto, null);

        foreach (var t in doneOrders)
        {
            if (!t.CanChooseQuantity)
                res += t.TotalPrice - t.Price * t.TotalQuantity;
            else
            {
                var perUnit = t.Price / t.Quantity;
                var rem = t.Quantity - t.TotalQuantity;
                res += rem * perUnit;
            }
        }

        return res;
    }
}