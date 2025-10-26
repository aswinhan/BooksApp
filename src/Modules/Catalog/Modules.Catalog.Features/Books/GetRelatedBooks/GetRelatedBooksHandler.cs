using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Catalog.Features.Books.Shared.Responses; // Use BookListResponse
using Modules.Catalog.Infrastructure.Database;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;

namespace Modules.Catalog.Features.Books.GetRelatedBooks;

internal interface IGetRelatedBooksHandler : IHandler
{
    Task<Result<List<BookListResponse>>> HandleAsync(Guid bookId, int count, CancellationToken cancellationToken);
}

internal sealed class GetRelatedBooksHandler(
    CatalogDbContext dbContext,
    ILogger<GetRelatedBooksHandler> logger)
    : IGetRelatedBooksHandler
{
    public async Task<Result<List<BookListResponse>>> HandleAsync(Guid bookId, int count, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving related books for Book {BookId}", bookId);

        // Ensure count is reasonable
        count = Math.Clamp(count, 1, 10);

        try
        {
            // 1. Find the original book to get its AuthorId and CategoryId
            var originalBook = await dbContext.Books
                                       .AsNoTracking()
                                       .Select(b => new { b.Id, b.AuthorId, b.CategoryId }) // Only select needed fields
                                       .FirstOrDefaultAsync(b => b.Id == bookId, cancellationToken);

            if (originalBook is null)
            {
                logger.LogWarning("Get related books failed: Original Book {BookId} not found.", bookId);
                return Error.NotFound("Catalog.BookNotFound", $"Book with ID {bookId} not found.");
            }

            // 2. Find related books (same author OR same category, excluding self)
            var relatedBooks = await dbContext.Books
                .AsNoTracking()
                .Include(b => b.Author) // Need Author for the response DTO
                .Where(b => b.Id != bookId && // Exclude the original book
                            (b.AuthorId == originalBook.AuthorId || b.CategoryId == originalBook.CategoryId))
                .OrderBy(b => Guid.NewGuid()) // Simple random ordering for variety, inefficient on large datasets
                                              // Alternative: Order by CreatedAtUtc descending or another metric
                .Take(count) // Limit the number of results
                .Select(b => new BookListResponse( // Project to DTO
                    b.Id,
                    b.Title,
                    b.Author.Name,
                    b.Price,
                    null, // Average Rating - could calculate here if needed
                    b.CoverImageUrl
                ))
                .ToListAsync(cancellationToken);

            logger.LogInformation("Retrieved {Count} related books for Book {BookId}", relatedBooks.Count, bookId);
            return relatedBooks; // Implicit conversion
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve related books for Book {BookId}.", bookId);
            return Error.Unexpected("Catalog.GetRelatedFailed", "Failed to retrieve related books.");
        }
    }
}