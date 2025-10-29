using MailKit.Net.Smtp; // For SmtpClient
using MailKit.Security; // For SecureSocketOptions
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options; // For IOptions
using MimeKit; // For MimeMessage, BodyBuilder
using Modules.Common.Application.Email; // Use IEmailService
using Modules.Common.Infrastructure.Configuration; // Use SmtpSettings
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Common.Infrastructure.Email;

public class MailKitEmailService(
    IOptions<SmtpSettings> smtpOptions, // Inject settings
    ILogger<MailKitEmailService> logger) : IEmailService
{
    private readonly SmtpSettings _settings = smtpOptions.Value;

    public async Task SendEmailAsync(string toEmail, string subject, string bodyHtml, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Attempting to send email to {ToEmail} with subject {Subject}", toEmail, subject);

        try
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = bodyHtml };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            var options = _settings.EnableSsl ? SecureSocketOptions.StartTlsWhenAvailable : SecureSocketOptions.None;
            await smtp.ConnectAsync(_settings.Host, _settings.Port, options, cancellationToken);

            // Authenticate only if username is provided
            if (!string.IsNullOrEmpty(_settings.Username) && !string.IsNullOrEmpty(_settings.Password))
            {
                await smtp.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
            }

            var response = await smtp.SendAsync(email, cancellationToken);
            await smtp.DisconnectAsync(true, cancellationToken);

            logger.LogInformation("Email sent to {ToEmail}. Response: {Response}", toEmail, response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {ToEmail}", toEmail);
            // Decide: re-throw, or just log? For now, just log.
            // Re-throwing might fail the operation (e.g., contact form submit)
            // throw;
        }
    }
}