using AutoMapper;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Extentions;
using ChargingApp.Helpers;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

[Authorize(Policy = "Required_JustVIP_Role")]
public class PaymentsController : BaseApiController
{
    private readonly List<string> _status = new()
        { "Pending", "Succeed", "Rejected", "Wrong", "Received", "Cancelled" };

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;
    private readonly IPhotoService _photoService;

    public PaymentsController(IUnitOfWork unitOfWork, IMapper mapper,
        INotificationService notificationService, IPhotoService photoService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notificationService = notificationService;
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

            var roles = User.GetRoles().ToList();
            if (SomeUsefulFunction.CheckIfItIsAnAdmin(roles))
                return BadRequest(new ApiResponse(403, "You're an admin... can't make an order with this account"));

            var agent = await _unitOfWork.PaymentRepository.GetPaymentAgentByIdAsync(agentId);

            if (agent is null)
                return BadRequest(new ApiResponse(400, "this agent isn't exist"));

            if (agent.RechargeMethodMethod.EnglishName.ToLower() != "companies")
                return BadRequest(new ApiResponse(400, "This agent not found"));

            var result = await _photoService.AddPhotoAsync(dto.ImageFile);
            if (!result.Success)
                return BadRequest(new ApiResponse(400, result.Message, "حدث خطأ أثناء تحميل الصورة"));

            var photo = new Photo
            {
                Url = result.Url
            };

            var payment = new Payment
            {
                User = user,
                AddedValue = dto.AddedValue,
                ClientDate = dto.CreatedDate.ToString(),
                Notes = dto.Notes,
                Username = dto.Username,
                PaymentAgentEnglishName = agent.EnglishName,
                PaymentAgentArabicName = agent.ArabicName,
                Photo = photo,
                PaymentType = "Companies",
            };

            _unitOfWork.PaymentRepository.AddPayment(payment);

            if (!await _unitOfWork.Complete())
            {
                return BadRequest(new ApiResponse(400, "Failed to add payment"));
            }

            var not = new OrderAndPaymentNotification
            {
                Payment = payment,
                User = user
            };
            await _notificationService.NotifyUserByEmail(payment.User.Email, _unitOfWork, not,
                "Payment status notification", getDetails(payment));

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

            var roles = User.GetRoles().ToList();
            if (SomeUsefulFunction.CheckIfItIsAnAdmin(roles))
                return BadRequest(new ApiResponse(403, "You're an admin... can't make an order with this account"));

            var agent = await _unitOfWork.PaymentRepository.GetPaymentAgentByIdAsync(agentId);

            if (agent is null)
                return BadRequest(new ApiResponse(400, "this agent isn't exist"));

            if (agent.RechargeMethodMethod.EnglishName.ToLower() != "offices")
                return BadRequest(new ApiResponse(400, "This agent not found"));

            var result = await _photoService.AddPhotoAsync(dto.ImageFile);
            if (!result.Success)
                return BadRequest(new ApiResponse(400, result.Message, "حدث خطأ أثناء تحميل الصورة"));

            var photo = new Photo
            {
                Url = result.Url
            };

            var payment = new Payment
            {
                User = user,
                AddedValue = dto.AddedValue,
                ClientDate = dto.CreatedDate.ToString(),
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
                var not = new OrderAndPaymentNotification
                {
                    Payment = payment,
                    User = user
                };
                await _notificationService.NotifyUserByEmail(payment.User.Email, _unitOfWork, not,
                    "Payment status notification", getDetails(payment));

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

            var roles = User.GetRoles().ToList();
            if (SomeUsefulFunction.CheckIfItIsAnAdmin(roles))
                return BadRequest(new ApiResponse(403, "You're an admin... can't make an order with this account"));

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
                return BadRequest(new ApiResponse(400, result.Message, "حدث خطأ أثناء تحميل الصورة"));

            var photo = new Photo
            {
                Url = result.Url
            };
            var payment = new Payment
            {
                User = user,
                AddedValue = dto.AddedValue,
                ClientDate = dto.CreatedDate.ToString(),
                Notes = dto.Notes,
                Photo = photo,
                PaymentType = name,
            };

            _unitOfWork.PaymentRepository.AddPayment(payment);

            if (!await _unitOfWork.Complete()) return BadRequest(new ApiResponse(400, "Failed to add payment"));

            var not = new OrderAndPaymentNotification
            {
                Payment = payment,
                User = user
            };
            await _notificationService.NotifyUserByEmail(payment.User.Email, _unitOfWork, not,
                "Payment status notification", getDetails(payment));

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
            var roles = User.GetRoles().ToList();
            if (SomeUsefulFunction.CheckIfItIsAnAdmin(roles))
                return BadRequest(new ApiResponse(403, "You're an admin... can't make an order with this account"));

            email = email.ToLower();

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

    private Dictionary<string, dynamic> getDetails(Payment order)
    {
        return new Dictionary<string, dynamic>
        {
            { "paymentId", order.Id },
            { "status", "payment " + _status[order.Status] }
        };
    }
}