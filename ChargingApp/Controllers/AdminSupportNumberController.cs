using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

public class AdminSupportNumberController : AdminController
{
    private readonly IUnitOfWork _unitOfWork;

    public AdminSupportNumberController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpPost("add-support-number")]
    public async Task<ActionResult> AddSupportNumber([FromQuery] string number)
    {
        try
        {
            var tmp = new SupportNumber
            {
                PhoneNumber = number
            };
            _unitOfWork.SupportNumberRepository.AddSupportNumber(tmp);
            if (await _unitOfWork.Complete())
                return Ok(new ApiOkResponse(tmp));
            return BadRequest(new ApiResponse(400, "Something went wrong"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpPut("update-support-number/{id:int}")]
    public async Task<ActionResult> AddSupportNumber([FromQuery] string number, int id)
    {
        var tmp = await _unitOfWork.SupportNumberRepository
            .GetSupportNumberByIdAsync(id);

        if (tmp is null)
            return BadRequest(new ApiResponse(400, "id not found"));

        tmp.PhoneNumber = number;
        if (await _unitOfWork.Complete())
            return Ok(new ApiOkResponse(tmp));
        
        return BadRequest(new ApiResponse(400, "Something went wrong"));
    }
}