using System.Net.Http.Headers;
using System.Text;
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
using Newtonsoft.Json;

namespace ChargingApp.Controllers;

public class HomeController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public HomeController(IUnitOfWork unitOfWork,IMapper mapper
    )
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<HomeDto>> GetHomePage()
    {
        var res = new HomeDto
        {
            Categories = await _unitOfWork.CategoryRepository.GetAllCategoriesAsync()
        };
        return res;
    }

}