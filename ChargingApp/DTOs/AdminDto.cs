namespace ChargingApp.DTOs;

public class AdminDtoInfo
{
}

public class AdminDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PhoneNumber { get; set; }
    public List<string> Roles { get; set; }
}

public class AdminLoginDto : AdminDto
{
    public string Token { get; set; }
}