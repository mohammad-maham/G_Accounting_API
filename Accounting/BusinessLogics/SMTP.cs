using Accounting.BusinessLogics.IBusinessLogics;
using Accounting.Models;
using System.Net;
using System.Net.Mail;

namespace Accounting.BusinessLogics
{
    public class SMTP : ISMTP
    {
        private readonly ILogger<SMTP> _logger;
        public SMTP(ILogger<SMTP> logger)
        {
            _logger = logger;
        }

        public async Task SendEmailAsync(SMTPModel smtp)
        {
            // Set up SMTP client
            SmtpClient client = new(smtp.Options!.Host, smtp.Options!.Port)
            {
                EnableSsl = smtp.Options!.EnableSSL,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(smtp.Options!.Username, smtp.Options!.Password)
            };

            // Create email message
            MailMessage mailMessage = new()
            {
                From = new MailAddress(smtp.Options!.FromEmail!)
            };
            mailMessage.To.Add(smtp.To!);
            mailMessage.Subject = smtp.Subject;
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = smtp.Body;

            // Send email
            await client.SendMailAsync(mailMessage);
        }
    }
}