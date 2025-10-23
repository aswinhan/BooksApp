using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Catalog.Features.Books.Shared.Responses;
using Modules.Catalog.Infrastructure.Database;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
// Add a new Paginated Response DTO (define below)
using Modules.Common.Application.Pagination; // Assuming PaginatedResponse lives in Common

namespace Modules.Catalog.Features.Books.GetBooksList;

// Update the interface to return the paginated response
internal interface IGetBooksListHandler : IHandler
{
    // Add pageNumber and pageSize parameters
    Task<Result<PaginatedResponse<BookListResponse>>> HandleAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
}

internal sealed class GetBooksListHandler(
    CatalogDbContext dbContext,
    ILogger<GetBooksListHandler> logger)
    : IGetBooksListHandler
{
    // Update method signature
    public async Task<Result<PaginatedResponse<BookListResponse>>> HandleAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving books list - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);

        // Ensure valid pagination parameters
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 5, 50); // Min 5, Max 50 items per page

        try
        {
            // Base query (can add filtering later)
            var query = dbContext.Books
                .AsNoTracking()
                .Include(b => b.Author)
                .OrderBy(b => b.Title);

            // Get total count BEFORE applying pagination
            int totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            var books = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BookListResponse(
                    b.Id,
                    b.Title,
                    b.Author.Name,
                    b.Price,
                    null
                ))
                .ToListAsync(cancellationToken);

            logger.LogInformation("Retrieved {Count} books for page {PageNumber}.", books.Count, pageNumber);

            // Create the paginated response object
            var paginatedResponse = new PaginatedResponse<BookListResponse>(
                books,
                totalCount,
                pageNumber,
                pageSize
            );

            return paginatedResponse; // Implicit conversion
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve books list for page {PageNumber}.", pageNumber);
            return Error.Unexpected("Catalog.GetListFailed", "Failed to retrieve book list.");
        }
    }
}