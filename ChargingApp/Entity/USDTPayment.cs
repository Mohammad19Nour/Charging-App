namespace ChargingApp.Entity;

public class USDTPayment  :BaseEntity
{
    public DateTime CreatedDate { get; set; }
    public string? Notes{ get; set; }
    public int AddedValue { get; set; }
    public bool Aproved { get; set; }
    public bool Scucced { get; set; }
    public string Username { get; set; }
    public AppUser User { get; set; }
}