namespace ChargingApp.DTOs;

public class DebitDto
{
    public string Username { get; set; }
    public string UserEmail { get; set; }
    public decimal DebitValue { get; set; }
    public DateTime Date { get; set; }
}