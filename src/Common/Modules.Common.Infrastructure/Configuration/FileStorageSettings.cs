namespace Modules.Common.Infrastructure.Configuration;

public record FileStorageSettings
{
    public const string SectionName = "FileStorage";
    // Path on the server where files will be saved (relative to wwwroot)
    public required string LocalBasePath { get; init; } = "uploads";
    // Base URL used to serve the files (relative to application base URL)
    public required string LocalServePrefix { get; init; } = "/uploads";
}