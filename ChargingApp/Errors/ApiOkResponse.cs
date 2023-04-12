namespace ChargingApp.Errors;

public class ApiOkResponse <T>: ApiResponse
{
    public T Result { get; }
    public int StatusCode { get; set; }
    public ApiOkResponse(T result) : base(200)
    {
        Result = result;
        StatusCode = 200;
    }
}