﻿using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

public class RechargeMethodsController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;

    public RechargeMethodsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    //  [Authorize(Policy = "RequiredVIPRole")]
    [HttpGet("recharge-methods-available")]
    public async Task<ActionResult<PaymentAndRechargeMethodDto>> GetAllRechargeMethods()
    {
        try
        {
            var res = new PaymentAndRechargeMethodDto();

            var forRecharge = await _unitOfWork.RechargeMethodeRepository.GetRechargeMethodsAsync();
            var forBoth = await _unitOfWork.PaymentGatewayRepository.GetPaymentGatewaysAsync();

            res.ForPaymentAndRecharge = forBoth;
            res.ForRecharge = forRecharge??new List<RechargeMethodDto>();
            return Ok(new ApiOkResponse(result: res));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    //[Authorize(Policy = "RequiredNormalRole")]

    [HttpGet("normal-recharge-methods")]
    public async Task<ActionResult<List<PaymentGateway>>> GetNormalRechargeMethods()
    {
        try
        {
            var res = await _unitOfWork.PaymentGatewayRepository.GetPaymentGatewaysAsync();

            return Ok(new ApiOkResponse(result: res));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}