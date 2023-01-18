namespace ChargingApp.DTOs;

public class DebitDto
{
    public string Username { get; set; }
    public string UserEmail { get; set; }
    public double DebitValue { get; set; }
    public DateTime Date { get; set; }
}