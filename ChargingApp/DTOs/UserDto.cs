namespace ChargingApp.DTOs;

public class UserInfo
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? City { get; set; }
    
    public string? Country { get; set; }
    public string? PhoneNumber { get; set; }
}

public class UpdateUserInfoDto : UserInfo
{
    
}
public class UserDto : UserInfo
{
    public string AccountType { get; set; }
    public string Email { get; set; }
    public string Token { get; set; }
    public WalletDto MyWallet { get; set; }
    
}

public class UserInfoDto : UserInfo
{ 
    
    public string Email { get; set; }
    public string AccountType { get; set; }
    
    public WalletDto MyWallet { get; set; }
}