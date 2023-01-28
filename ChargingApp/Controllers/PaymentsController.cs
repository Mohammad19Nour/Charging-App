using AutoMapper;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Extentions;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

[Authorize(Policy = "RequiredVIPRole")]
public class PaymentsController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPhotoService _photoService;

    public PaymentsController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _photoService = photoService;
    }


    [HttpPost("add-payment/company/{agentId:int}")]
    public async Task<ActionResult<PaymentDto>> AddPaymentComp(int agentId, [FromForm] NewCompanyPaymentDto dto)
    {
        try
        {
            var email = User.GetEmail();
            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(email);
            if (user == null) return Unauthorized(new ApiResponse(401));

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
            };

            _unitOfWork.PaymentRepository.AddPayment(payment);

            if (!await _unitOfWork.Complete()) return BadRequest(new ApiResponse(400, "Failed to add payment"));

            return Ok(new ApiOkResponse(_mapper.Map<CompanyPaymentDto>(payment)));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpPost("add-payment/office/{agentId:int}")]
    public async Task<ActionResult<PaymentDto>> AddPaymentOff(int agentId, [FromForm] NewOfficePaymentDto dto)
    {
        try
        {
            var email = User.GetEmail();
            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(email);
            if (user == null) return Unauthorized(new ApiResponse(401));

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
            };


            _unitOfWork.PaymentRepository.AddPayment(payment);

            if (await _unitOfWork.Complete())
            {
                return Ok(new ApiOkResponse(_mapper.Map<OfficePaymentDto>(payment)));
            }

            return BadRequest(new ApiResponse(400, "Failed to add payment"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpPost("add-payment/other/{name}")]
    public async Task<ActionResult<PaymentDto>> AddPaymentUsdt([FromForm] NewPaymentDto dto, string name)
    {
        try
        {
            var email = User.GetEmail();
            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(email);
            if (user == null) return Unauthorized(new ApiResponse(401));

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
            };

            _unitOfWork.PaymentRepository.AddPayment(payment);

            if (!await _unitOfWork.Complete()) return BadRequest(new ApiResponse(400, "Failed to add payment"));

            return Ok(new ApiOkResponse(_mapper.Map<PaymentDto>(payment)));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpGet("my-payments")]
    public async Task<ActionResult<List<CompanyPaymentDto>>> GetMyPayment()
    {
        try
        {
            var email = User.GetEmail();
            if (email is null) return Unauthorized(new ApiResponse(401));

            email = email?.ToLower();

            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(email);

            if (user == null) return Unauthorized(new ApiResponse(401));

            var res = await _unitOfWork.PaymentRepository.GetPaymentsForUserAsync(email);

            return Ok(new ApiOkResponse(res));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}