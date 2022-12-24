using System.Diagnostics;
using ChargingApp.DTOs;

namespace ChargingApp.Errors;

public class ApiResponse
{
    public ApiResponse(int statusCode, string message = null)
    {
        StatusCode = statusCode;
        Message = message ?? GetDefaultMessageForStatusCode(statusCode);
    }

    private static string GetDefaultMessageForStatusCode(int statusCode)
    {
        return statusCode switch
        {
            200 =>"Success" ,
            201 => "Created",
            400 => "A bad request, you have made",
            401 => "unauthorized",
            403 => "Sorry, you're forbidden from accessing this recourse, only admin can",
            404 => "Resource not found",
            500 => "Errors server",
            _ => "unknown code"
        };
    }

    public int StatusCode { get; set; }
    public string Message { get; set; }
}