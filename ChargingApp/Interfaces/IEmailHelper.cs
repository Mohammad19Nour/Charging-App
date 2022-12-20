namespace ChargingApp.Interfaces;

public interface IEmailHelper
{
    public Task<bool> SendEmailAsync(string email, string subject, string htmlMessage);
}