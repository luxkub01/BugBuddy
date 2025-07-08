using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace BugBuddy.Helpers
{
    public class EmailSender : IEmailSender
    {
        private readonly SmtpSettings _smtp;

        public EmailSender(IOptions<SmtpSettings> smtp)
        {
            _smtp = smtp.Value;

            // Log the loaded SMTP credentials for debugging
            Console.WriteLine("📧 [EmailSender] Username: " + _smtp.UserName);
            Console.WriteLine("🔐 [EmailSender] Password: " + _smtp.Password);
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var mail = new MailMessage
            {
                From = new MailAddress(_smtp.UserName, "BugBuddy Team"),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            mail.To.Add(email);

            using var client = new SmtpClient
            {
                Host = _smtp.Host,
                Port = _smtp.Port,
                EnableSsl = _smtp.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_smtp.UserName, _smtp.Password),
                Timeout = 10000 // optional but helps Gmail
            };

            await client.SendMailAsync(mail);
        }
    }
}
