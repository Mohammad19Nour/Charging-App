namespace ChargingApp.Entity;

public class HostingSite : BaseEntity
{
    public string SiteName { get; set; }
    public string BaseUrl { get; set; }
    public string Token { get; set; }
    
}