namespace ChargingApp.Errors;

public class ApiOkResponse : ApiResponse
{
    public object Result { get; }
    public int StatusCode { get; set; }
    public ApiOkResponse(object result) : base(200)
    {
        Result = result;
        StatusCode = 200;
    }
}