namespace ChargingApp.DTOs;

public class VipLevelInfo
{
    public decimal? BenefitPercent { get; set; }
    public decimal? Purchase { get; set; }
}

public class VipLevelDto
{
    public int VipLevel { get; set; }

    public decimal Purchase { get; set; }
    public string Photo { get; set; }
}

public class AdminVipLevelDto : VipLevelDto{
   public decimal BenefitPercent { get; set; }
}

public class NewVipLevel
{
    public int VipLevel { get; set; }
    public decimal BenefitPercent { get; set; }
    public decimal Purchase { get; set; } = 0;
    public IFormFile? ImageFile { get; set; }
}