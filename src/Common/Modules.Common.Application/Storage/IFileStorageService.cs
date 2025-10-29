using Microsoft.AspNetCore.Http; // Required for IFormFile
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Common.Application.Storage;

public interface IFileStorageService
{
    /// <summary>
    /// Saves a file to the configured storage location.
    /// </summary>
    /// <param name="file">The file to save (from an HTTP request).</param>
    /// <param name="containerName">A logical container/folder name (e.g., "book-covers").</param>
    /// <param name="blobName">Optional: A specific name for the file in storage. If null, a unique name is generated.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The publicly accessible URL of the saved file.</returns>
    Task<string> SaveFileAsync(IFormFile file, string containerName, string? blobName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file from storage.
    /// </summary>
    /// <param name="fileUrl">The URL of the file to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the base URL where files are served from.
    /// </summary>
    string GetBaseServeUrl(); // Needed to construct URLs
}