using Accounting.BusinessLogics.IBusinessLogics;
using Accounting.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Net;
using System.Net.Mail;

namespace Accounting.BusinessLogics
{
    public class SMTP : ISMTP
    {
        private readonly ILogger<SMTP>? _logger;
        private static readonly string[] Scopes = { GmailService.Scope.GmailSend, GmailService.Scope.GmailReadonly };
        private static readonly string ApplicationName = "Gold Marketing OAuth v1";

        public SMTP()
        {

        }

        public SMTP(ILogger<SMTP> logger)
        {
            _logger = logger;
        }

        public async Task SendEmailAsync(SMTPModel smtp)
        {
            try
            {
                // Set up SMTP client
                SmtpClient client = new(smtp.Options!.Host, smtp.Options!.Port)
                {
                    EnableSsl = smtp.Options!.EnableSSL,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(smtp.Options!.Username, smtp.Options!.Password),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
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
                client.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public async Task SendEmailViaGoogleApiAsync(SMTPModel smtp)
        {
            try
            {
                UserCredential credential;
                // Load client secrets.
                FileStream? stream =
                       new("Content/credentials.json", FileMode.Open, FileAccess.Read);

                string credPath = "token.json";
                ClientSecrets? secrets = GoogleClientSecrets.FromStream(stream).Secrets;
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true));
                Console.WriteLine("Credential file saved to: " + credPath);

                // Create Gmail API service.
                GmailService service = new(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName
                });

                // Define parameters of request.
                UsersResource.LabelsResource.ListRequest request = service.Users.Labels.List("me");

                // Send email
                _ = await request.ExecuteAsync();
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}