using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Catalog.Features.Books.Shared.Responses;
using Modules.Catalog.Infrastructure.Database;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;

namespace Modules.Catalog.Features.Books.UpdateBook;

internal interface IUpdateBookHandler : IHandler
{
    Task<Result<BookResponse>> HandleAsync(Guid bookId, UpdateBookRequest request, CancellationToken cancellationToken);
}

internal sealed class UpdateBookHandler(
    CatalogDbContext dbContext,
    ILogger<UpdateBookHandler> logger)
    : IUpdateBookHandler
{
    public async Task<Result<BookResponse>> HandleAsync(Guid bookId, UpdateBookRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to update book with ID: {BookId}", bookId);

        // 1. Find the Book Aggregate Root (include Author and Reviews for response mapping)
        var book = await dbContext.Books
            .Include(b => b.Author) // Needed for response
            .Include(b => b.Reviews) // Needed for response
            .FirstOrDefaultAsync(b => b.Id == bookId, cancellationToken);

        if (book is null)
        {
            logger.LogWarning("Update book failed: Book {BookId} not found.", bookId);
            return Error.NotFound("Catalog.BookNotFound", $"Book with ID {bookId} not found.");
        }

        // 2. Verify New Author Exists (if changed)
        if (book.AuthorId != request.AuthorId)
        {
            var authorExists = await dbContext.Authors.AnyAsync(a => a.Id == request.AuthorId, cancellationToken);
            if (!authorExists)
            {
                logger.LogWarning("Update book failed: New Author {AuthorId} not found.", request.AuthorId);
                return Error.NotFound("Catalog.AuthorNotFound", $"Author with ID {request.AuthorId} not found.");
            }
        }

        // 3. Check for ISBN Conflict (if ISBN changed)
        if (book.Isbn != request.Isbn)
        {
            var isbnExists = await dbContext.Books.AnyAsync(b => b.Id != bookId && b.Isbn == request.Isbn, cancellationToken);
            if (isbnExists)
            {
                logger.LogWarning("Update book failed: New ISBN {ISBN} already exists.", request.Isbn);
                return Error.Conflict("Catalog.IsbnExists", $"Book with ISBN {request.Isbn} already exists.");
            }
        }


        // 4. Execute the Domain Logic via the Aggregate Root
        try
        {
            book.UpdateDetails(
                request.Title,
                request.Description,
                request.Isbn,
                request.Price,
                request.AuthorId
            );
        }
        catch (ArgumentException ex) // Catch validation errors from domain
        {
            logger.LogWarning("Update book failed due to validation: {Reason}", ex.Message);
            return Error.Validation("Catalog.UpdateValidation", ex.Message);
        }
        catch (InvalidOperationException ex) // Catch potential domain rule violations
        {
            logger.LogWarning("Update book failed for Book {BookId}: {Reason}", bookId, ex.Message);
            return Error.Failure("Catalog.UpdateFailed", ex.Message);
        }

        // 5. Save changes
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully updated book with ID: {BookId}", bookId);

        // 6. Map updated entity to Response DTO
        // If AuthorId changed, we need to fetch the new Author's name for the response
        var authorName = book.AuthorId == request.AuthorId
                       ? book.Author.Name // Use existing loaded Author if ID didn't change
                       : (await dbContext.Authors.FindAsync([request.AuthorId], cancellationToken: cancellationToken))?.Name ?? "Unknown";


        var response = new BookResponse(
             book.Id, 
             book.Title, 
             book.Description, 
             book.Isbn, 
             book.Price, 
             book.AuthorId,
             authorName, // Use potentially updated author name
             book.Reviews.Select(r => new ReviewResponse(
                 r.Id, r.UserId, r.Comment, r.Rating.Value, r.CreatedAtUtc
             )).ToList(),
             // Fetch stock quantity here if needed for Update response
             0, // Placeholder for QuantityAvailable - Fetch from Inventory if needed
             book.CreatedAtUtc, 
             book.UpdatedAtUtc // UpdatedAtUtc will be set
         );

        return response;
    }
}