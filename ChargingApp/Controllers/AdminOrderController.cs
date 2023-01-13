using ChargingApp.DTOs;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

public class AdminOrderController : AdminController
{
  private readonly IUnitOfWork _unitOfWork;

  public AdminOrderController(IUnitOfWork unitOfWork)
  {
    _unitOfWork = unitOfWork;
  }

  [HttpPost("approve/{orderId:int}")]
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
  
  [HttpPost("reject/{orderId:int}")]
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
  
  [HttpGet("pending-orders")]
  public async Task<ActionResult<List<PendingOrderDto>>> GetUncheckedOrders()
  {
    var res = await _unitOfWork.OrdersRepository.GetPendingOrdersAsync();
    return Ok(new ApiOkResponse(res));
  }
  [HttpGet("pending-orders/{email}")]
  public async Task<ActionResult<List<PendingOrderDto>>> GetUncheckedOrders(string email)
  {
    var res = await _unitOfWork.OrdersRepository.GetPendingOrdersAsync(email);
    return Ok(new ApiOkResponse(res));
  }
/*
 0 not canceled
 1 canceled but not confirmed by admin
 2 canceled and accepted by admin
 3 canceled but rejected by admin
 */
  [HttpPost("approve-cancelation")]
  public async Task<ActionResult> ApproveCancel(int orderId)
  {
    var order = await _unitOfWork.OrdersRepository.GetOrderByIdAsync(orderId);
   
    if (order is null) return NotFound(new ApiResponse(404,"order not found"));

    if (order.StatusIfCanceled != 1 || order.Checked)
      return BadRequest(new ApiResponse(400, "can't cancel this order"));
    
    var price = order.TotalPrice;
    
    order.User.Balance += price;
    order.User.TotalPurchasing -= price;
    order.User.TotalForVIPLevel -= price;
    order.StatusIfCanceled = 2;

    order.User.VIPLevel =
      await _unitOfWork.VipLevelRepository.GetVipLevelForPurchasingAsync(order.User.TotalForVIPLevel);
    
    order.Notes = "accept";
    order.StatusIfCanceled = 2;
    
    if (await _unitOfWork.Complete()) return Ok(new ApiResponse(200, "approved"));
    return BadRequest(new ApiResponse(400, "Failed to cancel order"));
  }

  [HttpPost("reject-cancelation")]
  public async Task<ActionResult> RejectCancelation(int orderId)
  {
    var order = await _unitOfWork.OrdersRepository.GetOrderByIdAsync(orderId);
    if (order is null) return NotFound(new ApiResponse(404,"order not found"));
    
    if (order.StatusIfCanceled != 1 || order.Checked)
      return BadRequest(new ApiResponse(400, "can't cancel this order"));

    order.StatusIfCanceled = 3;
    order.Notes = "the cancelation order is rejected by admin";
    
    if (await _unitOfWork.Complete()) return Ok(new ApiResponse(200, "Rejected"));
    return BadRequest(new ApiResponse(400, "Failed to reject cancel order"));
  }

  [HttpGet("get-canceled-orders")]
  public async Task<ActionResult> CanceledOrders(string? userEmail)
  {

    if (userEmail != null)
    {
      var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(userEmail);

      if (user == null)
        return NotFound(new ApiResponse(404, "user not found"));
      
    }

    var orders = await _unitOfWork.OrdersRepository.GetCanceledOrdersRequestAsync(userEmail);

    return Ok(new ApiOkResponse(orders));
  }
}