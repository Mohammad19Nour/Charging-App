using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Charging_App.Services;

public class EmailSenderService :IEmailSender
{
    public EmailSenderService()
    {
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {

        var senderEmail = "chargingapp4@gmail.com";
        var senderPassword = "hmjupiwkrlufbuft" ;
        var senderUserName = "Charging App Team";
        
        MailMessage mail = new MailMessage();
        mail.To.Add(email);
        mail.From = new MailAddress(senderEmail);
        mail.Subject = subject;
        mail.Body =htmlMessage;
        mail.IsBodyHtml = true;
        SmtpClient smtp = new SmtpClient("smtp.gmail.com",587);
        smtp.EnableSsl = true;
        smtp.UseDefaultCredentials = false;
        smtp.Credentials = new System.Net.NetworkCredential(senderEmail, senderPassword);
        try
        {
            smtp.Send(mail);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}