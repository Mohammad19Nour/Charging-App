namespace ChargingApp.Entity;

public class Favorite
{
    public AppUser User { get; set; }
    public int UserId { get; set; }
    public Category Category { get; set; }
    public int CategoryId { get; set; }
}