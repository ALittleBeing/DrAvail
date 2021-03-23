using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace DrAvail.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration; 
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            var apiKey = _configuration.GetSection("SENDGRID_API_KEY").Value;
            return Execute(apiKey, subject, message, email);
        }

        public Task Execute(string apiKey, string subject, string message, string email)
        {
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                //email: DrAvail@outlook.com
                //password: Dr_avail@asp.net
                //sendGrid password: Dr_Avail@asp.net
                //API key Name: DrAvailFirstKey
                //API Key: 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
                From = new EmailAddress("DrAvail@outlook.com", "Dr Avail"),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(email));

            // Disable click tracking.
            // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
            msg.SetClickTracking(false, false);

            return client.SendEmailAsync(msg);
        }
    }
}
