using System.Net;
using System.Text.Json;
using Charging_App.Errors;

namespace Charging_App.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next , ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            _next(context);
        }
        catch (Exception e)
        {
            _logger.LogError(e ,e.Message);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var response = _env.IsDevelopment()
                ? new ApiException((int)HttpStatusCode.InternalServerError, e.Message, e.StackTrace.ToString()) 
                : new ApiResponse((int)HttpStatusCode.InternalServerError);

            var json = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);

        }
    }
}