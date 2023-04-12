using System.Diagnostics;
using ChargingApp.DTOs;

namespace ChargingApp.Errors;

public class ApiResponse
{
    public ApiResponse(int statusCode, string message = null,string arabicMessage = null)
    {
        StatusCode = statusCode;
        Message = message ?? GetDefaultMessageForStatusCode(statusCode);
        ArabicMessage = arabicMessage ?? GetDefaultArabicMessageForStatusCode(statusCode);
    }

    private static string GetDefaultMessageForStatusCode(int statusCode)
    {
        return statusCode switch
        {
            200 =>"Success" ,
            201 => "Created",
            400 => "A bad request, you have made",
            401 => "unauthorized",
            403 => "Sorry, you're forbidden from accessing this recourse",
            404 => "Resource not found",
            500 => "Errors server",
            405 => "The target method doesn't support the request methode",
            _ => "unknown code"
        };
    }
    private static string GetDefaultArabicMessageForStatusCode(int statusCode)
    {
        return statusCode switch
        {
            200 =>"تمت العملية بنجاح" ,
            201 => "تمت العملية بنجاح",
            400 => "حدثت مشكلة ما...الرجاء المحاولة لاحقا",
            401 => "يجب عليك تسجيل الدخول أولا",
            403 => "غير مسموح لك الوصول الى الوجهة المطلوبة",
            404 => "غير موجود",
            500 => "Errors server",
            405 => "The target method doesn't support the request methode",
            _ => "unknown code"
        };
    }

    public int StatusCode { get; set; }
    public string Message { get; set; }
    public string ArabicMessage { get; set; }
}