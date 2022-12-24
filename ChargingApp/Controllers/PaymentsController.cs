using AutoMapper;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Extentions;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

public class PaymentsController : BaseApiController
{
    private readonly IPaymentRepository _paymentRepo;
    private readonly IUserRepository _userRepo;
    private readonly IMapper _mapper;

    public PaymentsController(IPaymentRepository paymentRepo, IUserRepository userRepo
        , IMapper mapper
    )
    {
        _paymentRepo = paymentRepo;
        _userRepo = userRepo;
        _mapper = mapper;
    }

    [HttpPost("add-payment/company/{agentId:int}")]
    public async Task<ActionResult<PaymentDto>> AddPaymentComp(int agentId, [FromBody] NewCompanyPaymentDto dto)
    {
        var email = User.GetEmail();
        var user = await _userRepo.GetUserByEmailAsync(email);
        if (user == null) return Unauthorized(new ApiResponse(401));

        if (user.VIPLevel == 0)
            return BadRequest(new ApiResponse(403, "you can't do this action"));
        
        var agent = await _paymentRepo.GetPaymentAgentByIdAsync(agentId);

        if ( agent is null)
            return BadRequest(new ApiResponse(400, "this agent isn't exist"));

        if ((dto.SecretNumber is null) || (dto.ReceiptNumber is null))
            return BadRequest(new ApiResponse(400, "secret and notice number shouldn't be null "));

        var payment = new Payment
        {
            User = user,
            AddedValue = dto.AddedValue,
            CreatedDate = dto.CreatedDate,
            Notes = dto.Notes,
            Username = dto.Username,
            SecretNumber = dto.SecretNumber,
            ReceiptNumber = dto.ReceiptNumber,
            PaymentAgent = agent.EnglishName,
            PaymentType = "Companies"
        };

        _paymentRepo.AddPayment(payment);

        if (await _paymentRepo.SaveAllChangesAsync())
            return Ok(new ApiOkResponse(_mapper.Map<CompanyPaymentDto>(payment)));

        return BadRequest(new ApiResponse(400, "Failed to add payment"));
    }

    
    [HttpPost("add-payment/office/{agentId:int}")]
    public async Task<ActionResult<PaymentDto>> AddPaymentOff(int agentId, [FromBody] NewPaymentDto dto)
    {
        var email = User.GetEmail();
        var user = await _userRepo.GetUserByEmailAsync(email);
        if (user == null) return Unauthorized(new ApiResponse(401));

        if (user.VIPLevel == 0)
            return BadRequest(new ApiResponse(403, "you can't do this action"));

        var agent = await _paymentRepo.GetPaymentAgentByIdAsync(agentId);

        if ( agent is null)
            return BadRequest(new ApiResponse(400, "this agent isn't exist"));

        var payment = new Payment
        {
            User = user,
            AddedValue = dto.AddedValue,
            CreatedDate = dto.CreatedDate,
            Notes = dto.Notes,
            Username = dto.Username,
            SecretNumber = null,
            ReceiptNumber = null,
            PaymentAgent = agent.EnglishName,
            PaymentType = "Offices"
        };

        _paymentRepo.AddPayment(payment);

        if (await _paymentRepo.SaveAllChangesAsync())
            return Ok(new ApiOkResponse(_mapper.Map<OfficePaymentDto>(payment)));

        return BadRequest(new ApiResponse(400, "Failed to add payment"));
    }
    [HttpPost("add-payment/usdt")]
    public async Task<ActionResult<PaymentDto>> AddPaymentUsdt([FromBody] NewPaymentDto dto)
    {
        var email = User.GetEmail();
        var user = await _userRepo.GetUserByEmailAsync(email);
        if (user == null) return Unauthorized(new ApiResponse(401));

        if (user.VIPLevel == 0)
            return BadRequest(new ApiResponse(403, "you can't do this action"));
        
        var payment = new Payment
        {
            User = user,
            AddedValue = dto.AddedValue,
            CreatedDate = dto.CreatedDate,
            Notes = dto.Notes,
            Username = dto.Username,
            SecretNumber = null,
            ReceiptNumber = null,
            PaymentType = "USDT"
        };

        _paymentRepo.AddPayment(payment);

        if (await _paymentRepo.SaveAllChangesAsync())
            return Ok(new ApiOkResponse(_mapper.Map<PaymentDto>(payment)));

        return BadRequest(new ApiResponse(400, "Failed to add payment"));
    }
    
    [HttpGet("my-payments")]
    public async Task<ActionResult<List<CompanyPaymentDto>>> GetMyPayment()
    {
        var email = User.GetEmail();
        var user = await _userRepo.GetUserByEmailAsync(email);
        if (user == null) return Unauthorized(new ApiResponse(401));

        if (user.VIPLevel == 0)
            return BadRequest(new ApiResponse(403, "you have no access to do this recourse"));

        email = email.ToLower();
        return  Ok( new ApiOkResponse( await _paymentRepo.GetPaymentsForUserAsync(email)));
    }
}