namespace ChargingApp.DTOs;

public class UserInfoDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string AccountType { get; set; }
    public double Balance { get; set; }
    
    public string? City { get; set; }
    
    public string? Country { get; set; }
}
public class UserDto : UserInfoDto
{
    public string Token { get; set; }
    
}