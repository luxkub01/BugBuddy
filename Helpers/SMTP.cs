using System.Net.Mail;
using System.Net;
using BugBuddy.ViewModel;

namespace BugBuddy.Helpers
{
    public class SMTP
    {
        public static async Task<bool> SendEmailAsync(SMTPEmailObject emailObject)
        {
            try
            {
                using (var mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(emailObject.From);
                    mailMessage.To.Add(emailObject.To);
                    mailMessage.Subject = emailObject.Subject;
                    mailMessage.Body = emailObject.Body;
                    mailMessage.IsBodyHtml = true; // Set to false if you are sending plain text

                    using (var smtpClient = new SmtpClient(emailObject.Server, emailObject.Port))
                    {
                        smtpClient.Credentials = new NetworkCredential(emailObject.Username, emailObject.Password);
                        smtpClient.EnableSsl = true; // Set to true if your SMTP server requires SSL
                        await smtpClient.SendMailAsync(mailMessage);
                    }
                }

                return true; // Email sent successfully
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                return false; // Indicate failure
            }
        }
    }
}
