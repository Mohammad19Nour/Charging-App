using ChargingApp.Errors;
using ChargingApp.Extentions;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

[Authorize]
public class NotificationHistoryController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;

    public NotificationHistoryController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiOkResponse<List<(string ArabicDetails, string EnglishDetails)>>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<List<(string ArabicDetails, string EnglishDetails)>>> GetNotifications()
    {
        try
        {
            var email = User.GetEmail();

            if (email is null) return Unauthorized(new ApiResponse(401));

            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(email);
            if (user is null) return Unauthorized(new ApiResponse(401));

            var tmp = await _unitOfWork.NotificationRepository
                .GetNotificationHistoryByEmailAsync(email);

            List<(string ArabicDetails, string EnglishDetails)> res =
                tmp.Select(t => (t.ArabicDetails, t.EnglishDetails)).ToList();

            return Ok(new ApiOkResponse<List<(string ArabicDetails, string EnglishDetails)>>(res));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}