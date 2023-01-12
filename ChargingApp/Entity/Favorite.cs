namespace ChargingApp.Entity;

public class Favorite
{
    public AppUser User { get; set; }
    public int UserId { get; set; }
    public Product Product { get; set; }
    public int ProductId { get; set; }
}