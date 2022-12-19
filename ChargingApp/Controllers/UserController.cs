using AutoMapper;
using ChargingApp.Entity;
using ChargingApp.Extentions;
using ChargingApp.Data;
using ChargingApp.DTOs;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

[Authorize]
public class UserController : BaseApiController
{
    private readonly IUserRepository _userRepo;
    private readonly IMapper _mapper;
    private readonly IProductRepository _productRepo;
    private readonly IVipLevelRepository _vipRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly DataContext _context;
    private readonly IPaymentRepository _paymentRepo;
    private readonly IOrdersRepository _ordersRepo;

    public UserController(IUserRepository userRepo, IMapper mapper,
        IProductRepository productRepo, IVipLevelRepository vipRepo
        , ICategoryRepository categoryRepo,DataContext context
        , IPaymentRepository paymentRepo, IOrdersRepository ordersRepo)
    {
        _userRepo = userRepo;
        _mapper = mapper;
        _productRepo = productRepo;
        _vipRepo = vipRepo;
        _categoryRepo = categoryRepo;
        _context = context;
        _paymentRepo = paymentRepo;
        _ordersRepo = ordersRepo;
    }

    [HttpPut("update-user-info")]
    public async Task<ActionResult> UpdateUser(UpdateUserInfoDto updateUserInfoDto)
    {
        var email = User.GetEmail();
        var user = await _userRepo.GetUserByEmailAsync(email);

        Console.WriteLine(user.City + "\n\n**\n\n");
        Console.WriteLine(updateUserInfoDto.City + "\n\n**\n\n");
        user.FirstName = updateUserInfoDto.FirstName;
        Console.WriteLine(user.City + "\n\n**\n\n");

        //_userRepo.UpdateUserInfo(user);

        if (await _userRepo.SaveAllAsync()) return Ok(new ApiResponse(204));
        return BadRequest(new ApiResponse(400, "Failed to update"));
    }


    // [HttpDelete("cancel-order")]
    // public async Task<ActionResult> DeleteOrder(int orderId)
    // {
    //     var order = await _penddingOrderRepo.GetOrderById(orderId);
    //
    //     if (order == null)
    //         return NotFound(new ApiResponse(404, "The order is not found"));
    //    
    //     var user = await _userRepo.GetUserByEmailAsync(User.GetEmail());
    //     if (user == null || user.Id != order.UserId) return Unauthorized(new ApiResponse(401));
    //
    //     if (order.CreatedDate.AddHours(1).CompareTo(DateTime.Now) < 0)
    //         return BadRequest(new ApiResponse(400 ,
    //             "Can't cancel order after more than one hour"));
    //     
    //     await _penddingOrderRepo.DeleteOrderByIdAsync(order.Id);
    //     return Ok(new ApiResponse(200, "Order deleted successfully"));
    // }

   /* [HttpGet("all-pending-order")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllPendingOrders()
    {
        var user = _userRepo.GetUserByEmailAsync(User.GetEmail());

        if (user == null) return Unauthorized(new ApiResponse(401));

        var resultList = await _penddingOrderRepo.GetAllOrdersOfUserByIdAsync(user.Id);
        var listDto = new List<IEnumerable<OrderDto>>();

        foreach (var order in resultList)
        {

            foreach (var pendding in order.Products)
            {
              var pro =  await _productRepo.GetProductByIdAsync(pendding.ProdudctId);
            }
            var tmp = new OrderDto
            {
                Id = order.Id,
                TotalPrice = order.TotalPrice
            };
        }

    }*/

    // [HttpGet("balance")]
    // public async Task<ActionResult<int>> GEtBalance()
    // {
    //     var user = await _userRepo.GetUserByEmailAsync(User.GetEmail());
    //     return Ok(new ApiOkResponse(user.Balance));
    // }

    // [HttpGet("payments")]
    // public async Task<ActionResult<List<PaymentDto>>> GetAllPayments()
    // {
    //     var email = User.GetEmail();
    //     var res = await _paymentRepo.GetPaymentsForUserAsync(email);
    //
    //     var list = new List<PaymentDto>();
    //     foreach (var pay in res)
    //     {
    //         list.Add(new PaymentDto
    //         {
    //             Date = pay.PaymentDate,
    //             Value = pay.Value
    //         });
    //     }
    //
    //     return Ok(new ApiOkResponse(list));
    // }

    // private double CalcPrice(double productSellingPrice, double discount)
    // {
    //     return productSellingPrice - productSellingPrice * discount / 100.0;
    // }

    // private async Task<List<ProductDto>> GetProductWithIds(List<int> ids, int discount)
    // {
    //     var products = new List<ProductDto>();
    //
    //     foreach (var id in ids)
    //     {
    //         var res = await _productRepo.GetProductByIdAsync(id);
    //         var productDto = new ProductDto();
    //         productDto.Id = res.Id;
    //         productDto.Price = CalcPrice(res.Price, discount);
    //         productDto.Value = res.Value;
    //         productDto.EnglishName = res.EnglishName;
    //         products.Add(productDto);
    //     }
    //
    //     return products;
    // }
}