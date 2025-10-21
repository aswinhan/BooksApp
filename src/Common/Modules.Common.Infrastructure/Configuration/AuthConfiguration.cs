namespace Modules.Common.Infrastructure.Configuration;

// Using a record for immutable configuration properties
public record AuthConfiguration
{
    // 'required' ensures these are set in appsettings.json
    public required string Key { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
}