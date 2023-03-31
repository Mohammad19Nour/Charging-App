namespace ChargingApp.DTOs;

public class AccountInfoDto
{
    public int UserVipLevel { get; set; }
    public List<VipLevelDto> VipLevels { get; set; }
    public decimal PurchasingPercentForCurrentVipLevel { get; set; }


    
}