namespace Modules.Common.Infrastructure.Configuration;

// Holds SMTP configuration from appsettings
public record SmtpSettings
{
    public const string SectionName = "SmtpSettings"; // Section name in appsettings
    public required string Host { get; init; }
    public int Port { get; init; }
    public required string SenderEmail { get; init; } // "From" address
    public required string SenderName { get; init; }
    public required string Username { get; init; } // SMTP login username
    public required string Password { get; init; } // SMTP login password
    public bool EnableSsl { get; init; }
}