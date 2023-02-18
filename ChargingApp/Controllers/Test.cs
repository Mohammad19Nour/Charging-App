using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChargingApp.Controllers;

[Authorize]
public class Test : BaseApiController
{
    [HttpPost]
    public string Gt()
    {
        return "ok";
    }
}