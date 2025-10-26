using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Catalog.Domain.Entities;
using Modules.Catalog.Features.Books.Shared.Responses;
using Modules.Catalog.Infrastructure.Database;
// Add a new Paginated Response DTO (define below)
using Modules.Common.Application.Pagination; // Assuming PaginatedResponse lives in Common
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Catalog.Features.Books.GetBooksList;

// Add parameters for filtering and sorting to return the paginated response
internal interface IGetBooksListHandler : IHandler
{
    Task<Result<PaginatedResponse<BookListResponse>>> HandleAsync(
        int pageNumber, int pageSize, Guid? authorId, Guid? categoryId,
        decimal? minPrice, decimal? maxPrice, string? searchQuery, int? minRating,
        string? sortBy, string? sortOrder, CancellationToken cancellationToken);
}

internal sealed class GetBooksListHandler(
    CatalogDbContext dbContext,
    ILogger<GetBooksListHandler> logger)
    : IGetBooksListHandler
{
    public async Task<Result<PaginatedResponse<BookListResponse>>> HandleAsync(
        int pageNumber, int pageSize, Guid? authorId, Guid? categoryId,
        decimal? minPrice, decimal? maxPrice, string? searchQuery, int? minRating,
        string? sortBy, string? sortOrder, CancellationToken cancellationToken)
    {
        logger.LogInformation(
        "Retrieving books list - Page: {Page}, Size: {Size}, Author: {Author}, Category: {Category}, Price: {Min}-{Max}, Sort: {SortBy} {SortOrder}",
            pageNumber, pageSize, authorId, categoryId, minPrice, maxPrice, sortBy, sortOrder);

        // Ensure valid pagination parameters
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 5, 50); // Min 5, Max 50 items per page
        if (minRating.HasValue) minRating = Math.Clamp(minRating.Value, 1, 5);

        try
        {
            // --- Base Query ---
            IQueryable<Book> query = dbContext.Books
                .AsNoTracking()
                .Include(b => b.Author)
                .Include(b => b.Reviews); // Keep reviews included

            // --- Apply Filtering ---
            if (authorId.HasValue) query = query.Where(b => b.AuthorId == authorId.Value);
            if (categoryId.HasValue) query = query.Where(b => b.CategoryId == categoryId.Value);
            if (minPrice.HasValue) query = query.Where(b => b.Price >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(b => b.Price <= maxPrice.Value);
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                string searchTerm = $"%{searchQuery.Trim()}%";
                query = query.Where(b => EF.Functions.ILike(b.Title, searchTerm) ||
                                         EF.Functions.ILike(b.Author.Name, searchTerm) ||
                                         EF.Functions.ILike(b.Isbn, searchTerm));
            }
            // Apply Rating Filter (requires calculation) - Filter *after* projection
            // We'll calculate rating in projection and filter the results list later if needed by minRating

            // --- Apply Sorting (directly on IQueryable<Book> where possible) ---
            bool descending = sortOrder?.ToLowerInvariant() == "desc";
            string sortField = sortBy?.ToLowerInvariant() ?? "title"; // Default sort

            // Apply sorting that EF Core CAN translate directly
            query = sortField switch
            {
                "price" => descending ? query.OrderByDescending(b => b.Price) : query.OrderBy(b => b.Price),
                "newness" => descending ? query.OrderByDescending(b => b.CreatedAtUtc) : query.OrderBy(b => b.CreatedAtUtc),
                "title" => descending ? query.OrderByDescending(b => b.Title) : query.OrderBy(b => b.Title),
                // Exclude "rating" here, handle it after projection if needed
                _ => query.OrderBy(b => b.Title) // Default sort
            };

            // --- Project to Intermediate DTO with Calculated Rating ---
            var projectedQuery = query.Select(b => new // Project to intermediate anonymous object
            {
                BookItem = new BookListResponse(
                    b.Id,
                    b.Title,
                    b.Author.Name,
                    b.Price,
                    null, // Placeholder for rating, calculated next
                    b.CoverImageUrl // Placeholder for CoverImageUrl
                 ),
                AverageRating = b.Reviews.Any() ? b.Reviews.Average(r => r.Rating.Value) : (double?)null,
                BookData = b, // Include BookData to access CoverImageUrl in final mapping if needed, or map directly
                CreatedAt = b.CreatedAtUtc // Needed if sorting by newness wasn't the primary DB sort
            });

            // --- Apply Rating Filter (after projection) ---
            if (minRating.HasValue)
            {
                projectedQuery = projectedQuery.Where(p => p.AverageRating.HasValue && p.AverageRating >= minRating.Value);
            }


            // --- Apply Rating Sort (if specified) ---
            // Note: Applying OrderBy *after* projection might be less efficient for DB pagination
            // but is necessary for calculated fields if EF Core can't translate the Average() sort.
            if (sortField == "rating")
            {
                projectedQuery = descending
                    ? projectedQuery.OrderByDescending(p => p.AverageRating)
                    : projectedQuery.OrderBy(p => p.AverageRating);
            }


            // Get total count AFTER filtering
            int totalCount = await projectedQuery.CountAsync(cancellationToken);

            // Apply pagination AFTER filtering and sorting
            var results = await projectedQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            // Map final results (assign calculated rating)
            var books = results.Select(p => p.BookItem with { AverageRating = p.AverageRating }).ToList();


            logger.LogInformation("Retrieved {Count} books for page {PageNumber}.", books.Count, pageNumber);

            var paginatedResponse = new PaginatedResponse<BookListResponse>(
                books, totalCount, pageNumber, pageSize
            );

            return paginatedResponse;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve books list.");
            return Error.Unexpected("Catalog.GetListFailed", "Failed to retrieve book list.");
        }
    }
}