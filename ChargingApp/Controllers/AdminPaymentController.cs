using ChargingApp.DTOs;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

public class AdminPaymentController : AdminController
{
  private readonly IUnitOfWork _unitOfWork;

  public AdminPaymentController(IUnitOfWork unitOfWork)
  {
    _unitOfWork = unitOfWork;
  }

  [HttpPost("approve/{paymentId:int}")]
  public async Task<ActionResult> ApprovePayment(int paymentId)
  {
    var payment = await _unitOfWork.PaymentRepository.GetPaymentByIdAsync(paymentId);

    if (payment is null) return NotFound(new ApiResponse(401 , "this payment doesn't exist"));

    if (payment.Status!=0) return BadRequest(new ApiResponse(400, "this payment already checked"));

    payment.Status = 1;

    var mn = Math.Min(payment.AddedValue, payment.User.Debit);

    payment.User.Debit -= mn;
    payment.AddedValue -= mn;
    
    payment.User.Balance += payment.AddedValue;

    if (await _unitOfWork.Complete())
    {
      return Ok(new ApiResponse(200 , "approved successfully"));
    }

    return BadRequest(new ApiResponse(400, "Failed to approve payment"));
  }
  
  
  [HttpPost("reject/{paymentId:int}")]
  public async Task<ActionResult> RejectPayment(int paymentId, string notes = "")
  {
    var payment = await _unitOfWork.PaymentRepository.GetPaymentByIdAsync(paymentId);

    if (payment is null) return NotFound(new ApiResponse(401 , "this order doesn't exist"));

    if (payment.Status != 0) return BadRequest(new ApiResponse(400, "this order already checked"));

    payment.Status = 2;
    payment.Notes = notes;

    if (await _unitOfWork.Complete())
    {
      return Ok(new ApiResponse(200 , "Rejected successfully"));
    }
    return BadRequest(new ApiResponse(400, "Failed to reject order"));
  }
  
  [HttpGet("pending-payment")]
  public async Task<ActionResult<List<PaymentAdminDto>>> GetPendingPayments(string? user = null)
  {
    
    var res = await _unitOfWork.PaymentRepository.GetAllPendingPaymentsAsync(user);
    return Ok(new ApiOkResponse(res));
  }

}