using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.OpenApi.Exceptions;

namespace ChargingApp.Services;

public class EmailSenderService : IEmailSender
{
    private const string SenderEmail = "chargingapp4@gmail.com";
    private const string  SenderPassword = "hmjupiwkrlufbuft";
    // const string senderUserName = "ChargingApp Team";
    
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
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
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to send email to " + email);
           // throw ;
        }
    }
    
}