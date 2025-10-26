using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Catalog.Features.Books.Shared.Responses; // Use ReviewResponse
using Modules.Catalog.Infrastructure.Database;
using Modules.Common.Application.Pagination; // Use PaginatedResponse
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;

namespace Modules.Catalog.Features.Books.GetBookReviews;

internal interface IGetBookReviewsHandler : IHandler
{
    Task<Result<PaginatedResponse<ReviewResponse>>> HandleAsync(
        Guid bookId, int pageNumber, int pageSize, CancellationToken cancellationToken);
}

internal sealed class GetBookReviewsHandler(
    CatalogDbContext dbContext,
    ILogger<GetBookReviewsHandler> logger)
    : IGetBookReviewsHandler
{
    public async Task<Result<PaginatedResponse<ReviewResponse>>> HandleAsync(
        Guid bookId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving reviews for Book {BookId} - Page: {Page}, Size: {Size}",
            bookId, pageNumber, pageSize);

        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 5, 20); // Limit reviews per page

        try
        {
            // First check if the book exists
            var bookExists = await dbContext.Books
                                     .AsNoTracking()
                                     .AnyAsync(b => b.Id == bookId, cancellationToken);
            if (!bookExists)
            {
                logger.LogWarning("Get reviews failed: Book {BookId} not found.", bookId);
                return Error.NotFound("Catalog.BookNotFound", $"Book with ID {bookId} not found.");
            }

            // Query for reviews of the specific book
            var query = dbContext.Reviews
                .AsNoTracking()
                .Where(r => r.BookId == bookId)
                .OrderByDescending(r => r.CreatedAtUtc); // Show newest reviews first

            // Get total count for pagination
            int totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            var reviews = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ReviewResponse( // Project to DTO
                    r.Id,
                    r.UserId,
                    r.Comment,
                    r.Rating.Value, // Get value from Rating VO
                    r.CreatedAtUtc
                ))
                .ToListAsync(cancellationToken);

            logger.LogInformation("Retrieved {Count} reviews for Book {BookId}, page {Page}.",
                reviews.Count, bookId, pageNumber);

            var paginatedResponse = new PaginatedResponse<ReviewResponse>(
                reviews, totalCount, pageNumber, pageSize
            );

            return paginatedResponse;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve reviews for Book {BookId}.", bookId);
            return Error.Unexpected("Catalog.GetReviewsFailed", "Failed to retrieve reviews.");
        }
    }
}