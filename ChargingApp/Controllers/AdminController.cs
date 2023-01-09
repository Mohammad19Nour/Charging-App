using ChargingApp.DTOs;
using ChargingApp.Extentions;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;
//[Authorize (Policy = "RequiredAdminRole")]
public class AdminController :BaseApiController
{
  private readonly IPhotoService _photoService;
  private readonly IUnitOfWork _unitOfWork;

  public AdminController(IPhotoService photoService , IUnitOfWork unitOfWork)
  {
    _photoService = photoService;
    _unitOfWork = unitOfWork;
  }

  [HttpPost("payment/approve/{paymentId:int}")]
  public async Task<ActionResult> ApprovePayment(int paymentId)
  {
    var payment = await _unitOfWork.PaymentRepository.GetPaymentByIdAsync(paymentId);

    if (payment is null) return NotFound(new ApiResponse(401 , "this payment doesn't exist"));

    if (payment.Checked) return BadRequest(new ApiResponse(400, "this payment already approved"));

    payment.Checked = true;
    payment.Succeed = true;

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
  
  
  [HttpPost("payment/reject/{paymentId:int}")]
  public async Task<ActionResult> RejectPayment(int paymentId, string notes = "")
  {
    var payment = await _unitOfWork.PaymentRepository.GetPaymentByIdAsync(paymentId);

    if (payment is null) return NotFound(new ApiResponse(401 , "this order doesn't exist"));

    if (payment.Checked) return BadRequest(new ApiResponse(400, "this order already checked"));

    payment.Checked = true;
    payment.Succeed = false;
    payment.Notes = notes;

    if (await _unitOfWork.Complete())
    {
      return Ok(new ApiResponse(200 , "Rejected successfully"));
    }
    return BadRequest(new ApiResponse(400, "Failed to reject order"));
  }
  
  [HttpGet("payment/pending-payment")]
  public async Task<ActionResult<List<PaymentAdminDto>>> GetPendingPayments()
  {
    var res = await _unitOfWork.PaymentRepository.GetAllPendingPaymentsAsync();
    return Ok(new ApiOkResponse(res));
  }

  [HttpPost("order/approve/{orderId:int}")]
  public async Task<ActionResult> Approve(int orderId)
  {
    var order = await _unitOfWork.OrdersRepository.GetOrderByIdAsync(orderId);

    if (order is null) return NotFound(new ApiResponse(401 , "this order doesn't exist"));

    if (order.Checked) return BadRequest(new ApiResponse(400, "this order already checked"));

    order.Checked = true;
    order.Succeed = true;

    if (await _unitOfWork.Complete())
    {
      return Ok(new ApiResponse(200 , "approved successfully"));
    }

    return BadRequest(new ApiResponse(400, "Failed to approve order"));
  }
  
  
  [HttpPost("order/reject/{orderId:int}")]
  public async Task<ActionResult> Reject(int orderId, string notes = "")
  {
    var order = await _unitOfWork.OrdersRepository.GetOrderByIdAsync(orderId);

    if (order is null) return NotFound(new ApiResponse(401 , "this order doesn't exist"));

    if (order.Checked) return BadRequest(new ApiResponse(400, "this order already checked"));

    order.Checked = true;
    order.Succeed = false;
    order.Notes = notes;

    if (order.User != null)
    {
      if (order.OrderType.ToLower() == "vip")
      {
        var user = order.User;

        user.Balance += order.TotalPrice;
        user.TotalPurchasing -= order.TotalPrice;
        user.VIPLevel = await _unitOfWork.VipLevelRepository.GetVipLevelForPurchasingAsync(user.TotalPurchasing);
      }
    }

    if (await _unitOfWork.Complete())
    {
      return Ok(new ApiResponse(200 , "Rejected successfully"));
    }
    return BadRequest(new ApiResponse(400, "Failed to reject order"));
  }
  
  [HttpGet("orders/pending-orders")]
  public async Task<ActionResult<List<PendingOrderDto>>> GetUncheckedOrders()
  {
    var res = await _unitOfWork.OrdersRepository.GetUnprovedOrdersAsync();
    return Ok(new ApiOkResponse(res));
  }
}