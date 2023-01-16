namespace ChargingApp.DTOs;

public class AccountInfoDto
{
    public string UserVipLevel { get; set; }
    public WalletDto MyWallet { get; set; }
    public List<VipLevelDto> VipLevels { get; set; }


    
}