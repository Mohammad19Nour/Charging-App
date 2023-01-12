using AutoMapper;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

public class RechargeMethodsController : AdminController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RechargeMethodsController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpGet("recharge-methods-available")]
    public async Task<ActionResult<PaymentAndRechargeMethodDto>> GetAllRechargeMethods()
    {
        var res = new PaymentAndRechargeMethodDto();
        
        var forRecharge = await _unitOfWork.RechargeMethodeRepository.GetRechargeMethodsAsync();
        var forBoth = await _unitOfWork.PaymentGatewayRepository.GetPaymentGatewaysAsync();

        res.ForPaymentAndRecharge = forBoth;
        res.ForRecharge = forRecharge;
        return Ok(new ApiOkResponse(result: res));
    }
    
    [HttpGet("normal-recharge-methods")]
    public async Task<ActionResult<List<PaymentGateway>>> GetNormalRechargeMethods()
    {
        var res = await _unitOfWork.PaymentGatewayRepository.GetPaymentGatewaysAsync();

        return Ok(new ApiOkResponse(result: res));
    }

    [HttpPost("add-agent/{rechargeMethodId:int}")]
    public async Task<ActionResult> AddAgent(int rechargeMethodId, [FromBody] NewAgentDto dto)
    {
        if (rechargeMethodId == 1)
            return BadRequest(
                new ApiResponse(403, "you can't add to this method "));
       
        var rechargeMethod = await _unitOfWork.RechargeMethodeRepository
            .GetRechargeMethodByIdAsync(rechargeMethodId);
        
        if (rechargeMethod is null)
            return NotFound(new ApiResponse(404, "recharge method not found"));

        var agent = _mapper.Map<ChangerAndCompany>(dto);
        _unitOfWork.RechargeMethodeRepository.AddAgent(rechargeMethod, agent);

        if (await _unitOfWork.Complete())
            return Ok(new ApiOkResponse(_mapper.Map<AgentDto>(agent)));

        return BadRequest(new ApiResponse(400, "Failed to add a new agent"));
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteAgent(int agentId, int rechargeMethodId)
    {
        var method = await _unitOfWork.RechargeMethodeRepository.GetRechargeMethodByIdAsync(rechargeMethodId);

        if (method is null)
            return BadRequest(new ApiResponse(403, "Recharge method not found"));

        if (method.ChangerAndCompanies is null)
            return BadRequest(new ApiResponse(403, "Agent not found"));

        var agent = method.ChangerAndCompanies.FirstOrDefault(x => x.Id == agentId);

        if (agent is null)
            return BadRequest(new ApiResponse(403, "Agent not found"));

        _unitOfWork.RechargeMethodeRepository.DeleteAgent(agent);
        
        if (await _unitOfWork.Complete())
            return Ok(new ApiResponse(200, "Deleted successfully"));

        return BadRequest(new ApiResponse(400, "Failed to delete new agent"));
    }
}