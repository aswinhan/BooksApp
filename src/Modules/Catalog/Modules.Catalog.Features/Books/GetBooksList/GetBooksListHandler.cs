using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Catalog.Features.Books.Shared.Responses;
using Modules.Catalog.Infrastructure.Database;
// Add a new Paginated Response DTO (define below)
using Modules.Common.Application.Pagination; // Assuming PaginatedResponse lives in Common
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Catalog.Features.Books.GetBooksList;

// Add parameters for filtering and sorting to return the paginated response
internal interface IGetBooksListHandler : IHandler
{
    Task<Result<PaginatedResponse<BookListResponse>>> HandleAsync(
        int pageNumber,
        int pageSize,
        Guid? authorId, // Optional Author ID filter
        string? sortBy, // Optional sort field (e.g., "title", "price")
        string? sortOrder, // Optional sort order ("asc", "desc")
        CancellationToken cancellationToken);
}

internal sealed class GetBooksListHandler(
    CatalogDbContext dbContext,
    ILogger<GetBooksListHandler> logger)
    : IGetBooksListHandler
{
    // Update method signature
    public async Task<Result<PaginatedResponse<BookListResponse>>> HandleAsync(
        int pageNumber,
        int pageSize,
        Guid? authorId, // Accept authorId
        string? sortBy, // Accept sortBy
        string? sortOrder, // Accept sortOrder
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
        "Retrieving books list - Page: {PageNumber}, Size: {PageSize}, AuthorId: {AuthorId}, SortBy: {SortBy}, SortOrder: {SortOrder}",
        pageNumber, pageSize, authorId, sortBy, sortOrder);

        // Ensure valid pagination parameters
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 5, 50); // Min 5, Max 50 items per page

        try
        {
            // Start with the base query including the Author
            IQueryable<Book> query = dbContext.Books
                .AsNoTracking()
                .Include(b => b.Author);

            // --- Apply Filtering ---
            if (authorId.HasValue)
            {
                query = query.Where(b => b.AuthorId == authorId.Value);
            }
            // Add other filters here later (e.g., Category)
            // if (categoryId.HasValue)
            // {
            //     query = query.Where(b => b.CategoryId == categoryId.Value);
            // }

            // --- Apply Sorting ---
            // Determine sort direction (default to ascending)
            bool descending = sortOrder?.ToLowerInvariant() == "desc";

            // Apply sorting based on sortBy parameter
            query = sortBy?.ToLowerInvariant() switch
            {
                "price" => descending
                            ? query.OrderByDescending(b => b.Price)
                            : query.OrderBy(b => b.Price),
                "title" => descending
                            ? query.OrderByDescending(b => b.Title)
                            : query.OrderBy(b => b.Title),
                // Default sort by Title if sortBy is null or invalid
                _ => query.OrderBy(b => b.Title)
            };

            // Get total count AFTER filtering
            int totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination AFTER filtering and sorting
            var books = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BookListResponse(
                    b.Id,
                    b.Title,
                    b.Author.Name, // Author is already included
                    b.Price,
                    null
                ))
                .ToListAsync(cancellationToken);

            logger.LogInformation("Retrieved {Count} books for page {PageNumber}.", books.Count, pageNumber);

            var paginatedResponse = new PaginatedResponse<BookListResponse>(
                books,
                totalCount,
                pageNumber,
                pageSize
            );

            return paginatedResponse;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve books list for page {PageNumber}.", pageNumber);
            return Error.Unexpected("Catalog.GetListFailed", "Failed to retrieve book list.");
        }
    }
}