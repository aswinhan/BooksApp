using Modules.Common.Domain.Results; // Use Result<>
using Modules.Catalog.PublicApi.Contracts; // Use DTOs
using System; // For Guid
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Catalog.PublicApi;

/// <summary>
/// Defines the public contract for interacting with the Catalog module internally.
/// </summary>
public interface ICatalogModuleApi
{
    /// <summary>
    /// Gets basic details for a specific book.
    /// </summary>
    /// <param name="bookId">The ID of the book.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result containing BookDetailsDto or an Error.</returns>
    Task<Result<BookDetailsDto>> GetBookByIdAsync(Guid bookId, CancellationToken cancellationToken);

    // Add other methods here later if needed by other modules
    // e.g., Task<Result<Success>> CheckBookAvailabilityAsync(Guid bookId, int quantity, CancellationToken cancellationToken);
}