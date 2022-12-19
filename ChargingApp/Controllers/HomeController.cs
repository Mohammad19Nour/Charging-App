using AutoMapper;
using AutoMapper.QueryableExtensions;
using ChargingApp.Extentions;
using ChargingApp.Data;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChargingApp.Controllers;

public class HomeController : BaseApiController
{
    private readonly ICategoryRepository _categoryRepo;
    private readonly IOrdersRepository _orderRepo;
    private readonly DataContext _context;
    private readonly IProductRepository _productRepo;
    private readonly IVipLevelRepository _vipRepo;
    private readonly IUserRepository _userRepo;
    private readonly IMapper _mapper;

    public HomeController(ICategoryRepository categoryRepo, IOrdersRepository orderRepo
        , DataContext context, IProductRepository productRepo,
        IUserRepository userRepo,IMapper mapper
    )
    {
        _categoryRepo = categoryRepo;
        _orderRepo = orderRepo;
        _context = context;
        _productRepo = productRepo;
        _userRepo = userRepo;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<HomeDto>> GetHomePage()
    {

        var res = new HomeDto
        {
            Categories = await _categoryRepo.GetAllCategoriesAsync()
        };
        return res;
    }

}