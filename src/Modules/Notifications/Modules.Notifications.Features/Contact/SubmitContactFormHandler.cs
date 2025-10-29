using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Modules.Common.Application.Email;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using System.Threading;
using System.Threading.Tasks;
// using Modules.Common.Application.Email; // Add later for IEmailService

namespace Modules.Notifications.Features.Contact;

internal interface ISubmitContactFormHandler : IHandler { Task<Result<Success>> HandleAsync(SubmitContactFormRequest request, CancellationToken ct); }
internal sealed class SubmitContactFormHandler(
    IEmailService emailService,
    IConfiguration configuration,
    ILogger<SubmitContactFormHandler> logger) : ISubmitContactFormHandler
{
    public async Task<Result<Success>> HandleAsync(SubmitContactFormRequest req, CancellationToken ct)
    {
        logger.LogInformation("Contact form submitted: Name='{Name}', Email='{Email}', Message='{Message}'",
            req.Name, req.Email, req.Message);

        // --- ADD Email Sending Logic ---
        try
        {
            // Get admin email from config (e.g., appsettings.json or user secrets)
            string adminEmail = configuration["AdminEmail"] ?? "admin@booksapp.com"; // Fallback
            string subject = $"Contact Form Submission from {req.Name}";
            string bodyHtml = $"""
                <p>You have a new contact form submission:</p>
                <ul>
                    <li><strong>Name:</strong> {req.Name}</li>
                    <li><strong>Email:</strong> {req.Email}</li>
                </ul>
                <hr>
                <p><strong>Message:</strong></p>
                <p>{req.Message.Replace("\n", "<br />")}</p>
                """;

            await emailService.SendEmailAsync(adminEmail, subject, bodyHtml, ct);
            logger.LogInformation("Contact form email sent successfully to {AdminEmail}.", adminEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send contact form email.");
            // Do not fail the request for the user, just log the error
            // return Error.Unexpected("Notifications.EmailFailed", "Failed to send message.");
        }
        // --- End Send Email ---

        return Result.Success;
    }
}