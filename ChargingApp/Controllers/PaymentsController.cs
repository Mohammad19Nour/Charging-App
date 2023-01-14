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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPhotoService _photoService;

    public PaymentsController(IUnitOfWork unitOfWork, IMapper mapper , IPhotoService photoService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _photoService = photoService;
    }

    // [Authorize(Policy = "RequiredVIPRole")]
    [HttpPost("add-payment/company/{agentId:int}")]
    public async Task<ActionResult<PaymentDto>> AddPaymentComp(int agentId, [FromForm] NewCompanyPaymentDto dto)
    {
        var email = User.GetEmail();
        var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(email);
        if (user == null) return Unauthorized(new ApiResponse(401));

        if (user.VIPLevel == 0)
            return BadRequest(new ApiResponse(403, "you can't do this action"));

        var agent = await _unitOfWork.PaymentRepository.GetPaymentAgentByIdAsync(agentId);

        if (agent is null)
            return BadRequest(new ApiResponse(400, "this agent isn't exist"));

        var result = await _photoService.AddPhotoAsync(dto.ImageFile);
        if (!result.Success)
            return BadRequest(new ApiResponse(400, result.Message));

        var photo = new Photo
        {
            Url = result.Url
        };
        
        var payment = new Payment
        {
            User = user,
            AddedValue = dto.AddedValue,
            CreatedDate = dto.CreatedDate,
            Notes = dto.Notes,
            Username = dto.Username,
            PaymentAgentEnglishName = agent.EnglishName,
            PaymentAgentArabicName = agent.ArabicName,
            Photo = photo,
            PaymentType = "Companies",
            //Succeed = true,
            //Checked = true
        };

       // user.Balance += payment.AddedValue;
        _unitOfWork.PaymentRepository.AddPayment(payment);
        
        if (!await _unitOfWork.Complete()) return BadRequest(new ApiResponse(400, "Failed to add payment"));
      
        return Ok(new ApiOkResponse(_mapper.Map<CompanyPaymentDto>(payment)));
    }


    //[Authorize(Policy = "RequiredVIPRole")]
    [HttpPost("add-payment/office/{agentId:int}")]
    public async Task<ActionResult<PaymentDto>> AddPaymentOff(int agentId, [FromForm] NewOfficePaymentDto dto)
    {
        var email = User.GetEmail();
        var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(email);
        if (user == null) return Unauthorized(new ApiResponse(401));

        if (user.VIPLevel == 0)
            return BadRequest(new ApiResponse(403, "you can't do this action"));

        var agent = await _unitOfWork.PaymentRepository.GetPaymentAgentByIdAsync(agentId);

        if (agent is null)
            return BadRequest(new ApiResponse(400, "this agent isn't exist"));

        var result = await _photoService.AddPhotoAsync(dto.ImageFile);
        if (!result.Success)
            return BadRequest(new ApiResponse(400, result.Message));

        var photo = new Photo
        {
            Url = result.Url
        };
        
        var payment = new Payment
        {
            User = user,
            AddedValue = dto.AddedValue,
            CreatedDate = dto.CreatedDate,
            Notes = dto.Notes,
            Username = dto.Username,
            PaymentAgentEnglishName = agent.EnglishName,
            PaymentAgentArabicName = agent.ArabicName,
            Photo = photo,
            PaymentType = "Offices",
          //  Succeed = true,
          //  Checked = true
        };

        //user.Balance += payment.AddedValue;
        
        _unitOfWork.PaymentRepository.AddPayment(payment);

        if (await _unitOfWork.Complete())
        {
            return Ok(new ApiOkResponse(_mapper.Map<OfficePaymentDto>(payment)));
        }

        return BadRequest(new ApiResponse(400, "Failed to add payment"));
    }


    //  [Authorize(Policy = "RequiredVIPRole")]
    [HttpPost("add-payment/other/{name}")]
    public async Task<ActionResult<PaymentDto>> AddPaymentUsdt([FromForm] NewPaymentDto dto, string name)
    {
        var email = User.GetEmail();
        var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(email);
        if (user == null) return Unauthorized(new ApiResponse(401));

        if (user.VIPLevel == 0)
            return BadRequest(new ApiResponse(403, "you can't do this action"));

        name = name.ToLower();
        
        var way = await _unitOfWork.PaymentGatewayRepository.GetPaymentGatewayByNameAsync(name);
        if (way != null)
        {
            way.EnglishName = way.EnglishName.ToLower();

            if (way.EnglishName is "companies" or "offices")
                return BadRequest(new ApiResponse(400, "you can't use this method"));
        }
        else 
            return BadRequest(new ApiResponse(400, "can't find the target method"));

        var result = await _photoService.AddPhotoAsync(dto.ImageFile);
        if (!result.Success)
            return BadRequest(new ApiResponse(400, result.Message));

        var photo = new Photo
        {
            Url = result.Url
        };
        var payment = new Payment
        {
            User = user,
            AddedValue = dto.AddedValue,
            CreatedDate = dto.CreatedDate,
            Notes = dto.Notes,
            Photo = photo,
            PaymentType = name,
            //Succeed = true,
            //Checked = true
        };

        _unitOfWork.PaymentRepository.AddPayment(payment);
      //  user.Balance += payment.AddedValue;
        
        if (!await _unitOfWork.Complete()) return BadRequest(new ApiResponse(400, "Failed to add payment"));
        
        return Ok(new ApiOkResponse(_mapper.Map<PaymentDto>(payment)));

    }


//    [Authorize(Policy = "RequiredVIPRole")]
    [HttpGet("my-payments")]
    public async Task<ActionResult<List<CompanyPaymentDto>>> GetMyPayment()
    {
        var email = User.GetEmail();
        if (email is null) return Unauthorized(new ApiResponse(401));

        email = email?.ToLower();

        var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(email);

        if (user == null) return Unauthorized(new ApiResponse(401));

        if (user.VIPLevel == 0)
            return BadRequest(new ApiResponse(403, "you have no access to do this recourse"));

        var res = await _unitOfWork.PaymentRepository.GetPaymentsForUserAsync(email);

        return Ok(new ApiOkResponse(res));
    }
}