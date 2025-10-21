using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // For FirstOrDefaultAsync, Include
using Microsoft.Extensions.Logging;
using Modules.Catalog.Domain.Entities; // For Book
using Modules.Catalog.Domain.ValueObjects; // For Rating value object
using Modules.Catalog.Infrastructure.Database; // For CatalogDbContext
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results; // For Result<>, Success, Error

namespace Modules.Catalog.Features.Books.AddBookReview;

// Interface for the handler
internal interface IAddBookReviewHandler : IHandler
{
    Task<Result<Success>> HandleAsync(Guid bookId, string userId, AddBookReviewRequest request, CancellationToken cancellationToken);
}

// Implementation of the handler
internal sealed class AddBookReviewHandler(
    CatalogDbContext dbContext, // Use DbContext directly for aggregate operations
    ILogger<AddBookReviewHandler> logger)
    : IAddBookReviewHandler
{
    public async Task<Result<Success>> HandleAsync(
        Guid bookId,
        string userId,
        AddBookReviewRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to add review for Book {BookId} by User {UserId}", bookId, userId);

        // 1. Find the Book Aggregate Root (including existing reviews)
        var book = await dbContext.Books
            .Include(b => b.Reviews) // MUST include reviews to check business rule
            .FirstOrDefaultAsync(b => b.Id == bookId, cancellationToken);

        if (book is null)
        {
            logger.LogWarning("Add review failed: Book {BookId} not found.", bookId);
            return Error.NotFound("Catalog.BookNotFound", $"Book with ID {bookId} not found.");
        }

        // 2. Create the Rating Value Object
        Rating ratingVO;
        try
        {
            ratingVO = Rating.Create(request.Rating);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            logger.LogWarning("Add review failed: Invalid rating value {Rating}.", request.Rating);
            // Return a validation error using our standard Error type
            return Error.Validation("Catalog.InvalidRating", ex.Message);
        }

        // 3. Execute the Domain Logic via the Aggregate Root
        try
        {
            book.AddReview(userId, request.Comment, ratingVO);
        }
        catch (InvalidOperationException ex) // Catch domain rule violations
        {
            logger.LogWarning("Add review failed for Book {BookId} by User {UserId}: {Reason}", bookId, userId, ex.Message);
            // Return a conflict error
            return Error.Conflict("Catalog.UserAlreadyReviewed", ex.Message);
        }

        // 4. Save changes (EF Core tracks the added review automatically)
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully added review for Book {BookId} by User {UserId}", bookId, userId);
        return Result.Success; // Return success (no value needed)
    }
}