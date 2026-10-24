using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SoccerTournament.Utility
{
    public sealed class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var host = _configuration["Email:Smtp:Host"];
            var from = _configuration["Email:From"];
            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
            {
                throw new InvalidOperationException(
                    "Email is not configured. Set Email:Smtp:Host and Email:From using application settings or environment variables.");
            }

            if (!int.TryParse(_configuration["Email:Smtp:Port"], out var port) || port is < 1 or > 65535)
            {
                throw new InvalidOperationException("Email:Smtp:Port must be a valid TCP port.");
            }

            using var message = new MailMessage(from, email, subject, htmlMessage)
            {
                IsBodyHtml = true
            };
            using var client = new SmtpClient(host, port)
            {
                EnableSsl = bool.TryParse(_configuration["Email:Smtp:EnableSsl"], out var enableSsl) && enableSsl
            };

            var username = _configuration["Email:Smtp:Username"];
            var password = _configuration["Email:Smtp:Password"];
            if (!string.IsNullOrWhiteSpace(username))
            {
                client.Credentials = new NetworkCredential(username, password);
            }

            try
            {
                await client.SendMailAsync(message);
            }
            catch (SmtpException exception)
            {
                _logger.LogError(exception, "SMTP delivery failed for recipient {Recipient}.", email);
                throw;
            }
        }

        public async Task SendTeamMessageAsync(
            IEnumerable<string> recipients,
            string subject,
            string textBody)
        {
            var recipientList = recipients
                .Where(recipient => !string.IsNullOrWhiteSpace(recipient))
                .Select(recipient => recipient.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (recipientList.Length == 0)
            {
                throw new InvalidOperationException("No roster members have an email address.");
            }

            var encodedSubject = WebUtility.HtmlEncode(subject);
            var encodedBody = WebUtility.HtmlEncode(textBody).Replace("\r\n", "<br>").Replace("\n", "<br>");
            var html = $"<h2>{encodedSubject}</h2><p>{encodedBody}</p>";
            foreach (var recipient in recipientList)
            {
                await SendEmailAsync(recipient, subject, html);
            }
        }
    }
}
