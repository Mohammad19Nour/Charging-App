namespace ChargingApp.Errors;

public class ApiValidationResponse : ApiResponse
{
    public ApiValidationResponse() : base(400)
    {
        
    }
    public IEnumerable<string>Error { get; set; }
}