using System.Net.Mail;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.OpenApi.Exceptions;

namespace ChargingApp.Services;

public class EmailSenderService : IEmailHelper
{
    private const string SenderEmail = "mohammad09nour@gmail.com";
    private const string  SenderPassword = "jirtpxwxhxwybooz";
    // const string senderUserName = "ChargingApp Team";
    
    public async Task <bool> SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var mail = new MailMessage();
        mail.To.Add(email);
        mail.From = new MailAddress(SenderEmail);
        mail.Subject = subject;
        mail.Body = htmlMessage;
        mail.IsBodyHtml = true;
        
        var smtp = new SmtpClient("smtp.gmail.com", 587);
        smtp.EnableSsl = true;
        smtp.UseDefaultCredentials = false;
        smtp.Credentials = new System.Net.NetworkCredential(SenderEmail, SenderPassword);

        try
        {
             await Task.Run(() => smtp.Send(mail));
             return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine("Failed to send email to " + email);
            return false;
            // throw ;
        }
    }
    
}