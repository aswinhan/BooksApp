using Microsoft.AspNetCore.Hosting; // For IWebHostEnvironment
using Microsoft.AspNetCore.Http; // For IFormFile
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modules.Common.Application.Storage; // Use interface
using Modules.Common.Infrastructure.Configuration; // Use settings
using System;
using System.IO; // For Path, File, Directory
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Common.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment; // To get wwwroot path
    private readonly FileStorageSettings _settings;
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly string _storageRootPath; // Absolute path to storage folder
    private readonly string _serveUrlBase; // Base URL prefix

    public LocalFileStorageService(
        IWebHostEnvironment environment,
        IOptions<FileStorageSettings> options,
        ILogger<LocalFileStorageService> logger)
    {
        _environment = environment;
        _settings = options.Value;
        _logger = logger;

        // Combine wwwroot path with the configured base path
        _storageRootPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, _settings.LocalBasePath);
        // Ensure the base URL prefix starts and ends correctly
        _serveUrlBase = _settings.LocalServePrefix.StartsWith('/') ? _settings.LocalServePrefix : "/" + _settings.LocalServePrefix;
        _serveUrlBase = _serveUrlBase.EndsWith('/') ? _serveUrlBase.TrimEnd('/') : _serveUrlBase;

        // Ensure the storage directory exists
        if (!Directory.Exists(_storageRootPath))
        {
            Directory.CreateDirectory(_storageRootPath);
            _logger.LogInformation("Created local storage directory: {Path}", _storageRootPath);
        }
        else
        {
            _logger.LogDebug("Local storage directory already exists: {Path}", _storageRootPath);
        }
    }

    public string GetBaseServeUrl() => _serveUrlBase;

    public async Task<string> SaveFileAsync(IFormFile file, string containerName, string? blobName = null, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File cannot be null or empty.", nameof(file));
        }
        if (string.IsNullOrWhiteSpace(containerName))
        {
            throw new ArgumentException("Container name cannot be empty.", nameof(containerName));
        }

        // Sanitize container name (remove invalid path characters)
        containerName = SanitizePathPart(containerName);
        var containerPath = Path.Combine(_storageRootPath, containerName);

        // Ensure container directory exists
        if (!Directory.Exists(containerPath))
        {
            Directory.CreateDirectory(containerPath);
        }

        // Generate unique filename if blobName is not provided
        var fileExtension = Path.GetExtension(file.FileName);
        blobName = string.IsNullOrWhiteSpace(blobName)
                 ? $"{Guid.NewGuid()}{fileExtension}"
                 : SanitizePathPart(blobName); // Sanitize provided blob name

        var filePath = Path.Combine(containerPath, blobName);

        try
        {
            _logger.LogInformation("Saving file to: {FilePath}", filePath);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            // Construct the publicly accessible URL
            string fileUrl = $"{_serveUrlBase}/{containerName}/{blobName}";
            _logger.LogInformation("File saved successfully. URL: {FileUrl}", fileUrl);
            return fileUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save file {FileName} to {FilePath}", file.FileName, filePath);
            throw; // Re-throw exception after logging
        }
    }

    public Task DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            return Task.CompletedTask; // Nothing to delete
        }

        try
        {
            // Convert URL back to file path
            if (!fileUrl.StartsWith(_serveUrlBase, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Cannot delete file: URL {FileUrl} does not match serve prefix {ServePrefix}", fileUrl, _serveUrlBase);
                return Task.CompletedTask;
            }

            var relativePath = fileUrl.Substring(_serveUrlBase.Length).TrimStart('/');
            var filePath = Path.Combine(_storageRootPath, relativePath);

            if (File.Exists(filePath))
            {
                _logger.LogInformation("Deleting file: {FilePath}", filePath);
                File.Delete(filePath);
            }
            else
            {
                _logger.LogWarning("File not found for deletion: {FilePath}", filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file corresponding to URL: {FileUrl}", fileUrl);
            // Don't throw, deletion failure might not be critical
        }

        return Task.CompletedTask;
    }

    // Basic sanitization for path parts
    private static string SanitizePathPart(string pathPart)
    {
        var invalidChars = Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()).Distinct();
        return string.Concat(pathPart.Split(invalidChars.ToArray(), StringSplitOptions.RemoveEmptyEntries)).Trim();
    }
}