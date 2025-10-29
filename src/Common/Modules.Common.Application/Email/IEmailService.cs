using System.Threading;
using System.Threading.Tasks;

namespace Modules.Common.Application.Email;

public interface IEmailService
{
    /// <summary>
    /// Sends an email.
    /// </summary>
    /// <param name="toEmail">The recipient's email address.</param>
    /// <param name="subject">The subject line of the email.</param>
    /// <param name="bodyHtml">The HTML content of the email body.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendEmailAsync(string toEmail, string subject, string bodyHtml, CancellationToken cancellationToken = default);
}