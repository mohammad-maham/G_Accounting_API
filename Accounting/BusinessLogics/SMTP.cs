using Accounting.BusinessLogics.IBusinessLogics;
using Accounting.Models;
using Google.Apis.Gmail.v1;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net;
using System.Net.Mail;

namespace Accounting.BusinessLogics
{
    public class SMTP : ISMTP
    {
        private readonly ILogger<SMTP>? _logger;
        private readonly IConfiguration _config;
        private static readonly string[] Scopes = { GmailService.Scope.GmailSend, GmailService.Scope.GmailReadonly };

        public SMTP()
        {
            _config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
        }

        public SMTP(ILogger<SMTP> logger)
        {
            _logger = logger;
            _config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
        }

        public void SendEmail(SMTPModel smtp)
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
                client.SendMailAsync(mailMessage);
                client.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void SendAsanakSMS(SMSModel sms)
        {
            try
            {
                // BaseURL
                RestClient client = new($"{sms.Options!.Host}/sendsms");
                RestRequest request = new()
                {
                    Method = Method.Post
                };

                // Parameters
                request.AddQueryParameter("username", sms.Options.Username);
                request.AddQueryParameter("password", sms.Options.Password);
                request.AddQueryParameter<long>("source", sms.Options.Source!.Value);
                request.AddQueryParameter<long>("destination", sms.Destination!.Value);
                request.AddQueryParameter("message", sms.Options.Message);

                // Headers
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                request.AddHeader("cache-control", "no-cache");

                // Send SMS
                RestResponse response = client.ExecutePost(request);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void SendGoldOTPSMS(SMSModel sms)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build()
                .GetSection("ApiUrls");

            string host = config.GetValue<string>("CustomerCommunication")!;

            try
            {
                // BaseURL
                RestClient client = new($"{host}/api/Communications/SendNotification");
                RestRequest request = new()
                {
                    Method = Method.Post
                };

                // Parameters
                request.AddJsonBody(new
                {
                    NotifTypes = 105,
                    NotifBody = sms.Options!.Message,
                    SenderUserId = 1,
                    NotifUnit = 1,
                    RecieverUserId = sms.UserId
                });

                var x = JsonConvert.SerializeObject(new
                {
                    NotifTypes = 105,
                    NotifBody = sms.Options!.Message,
                    SenderUserId = 1,
                    NotifUnit = 1,
                    RecieverUserId = sms.UserId
                });


                // Headers
                request.AddHeader("content-type", "application/json");
                request.AddHeader("cache-control", "no-cache");

                // Send SMS
                RestResponse response = client.ExecutePost(request);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}