using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Catalog.Infrastructure.Database; // For CatalogDbContext
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results; // For Result<>, Success, Error

namespace Modules.Catalog.Features.Books.DeleteBook;

internal interface IDeleteBookHandler : IHandler
{
    Task<Result<Success>> HandleAsync(Guid bookId, CancellationToken cancellationToken);
}

internal sealed class DeleteBookHandler(
    CatalogDbContext dbContext,
    ILogger<DeleteBookHandler> logger)
    : IDeleteBookHandler
{
    public async Task<Result<Success>> HandleAsync(Guid bookId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to delete book with ID: {BookId}", bookId);

        // 1. Find the Book
        // Include Reviews if cascade delete is not configured or if you need to check rules
        var book = await dbContext.Books
             // .Include(b => b.Reviews) // Only needed if cascading delete isn't set up in DB/EF
             .FirstOrDefaultAsync(b => b.Id == bookId, cancellationToken);

        if (book is null)
        {
            logger.LogWarning("Delete book failed: Book {BookId} not found.", bookId);
            // Return NotFound even if delete "succeeds" idempotently
            return Error.NotFound("Catalog.BookNotFound", $"Book with ID {bookId} not found.");
        }

        // Optional: Add authorization checks here if not handled by endpoint policy
        // e.g., if (currentUser.Id != book.CreatedById && !currentUser.IsAdmin) return Error.Forbidden(...)

        // 2. Remove the book from the DbContext
        dbContext.Books.Remove(book);

        // Note: If you haven't configured cascade delete in EF Core or the database,
        // you would need to manually remove associated Reviews here first.
        // Example: dbContext.Reviews.RemoveRange(book.Reviews);

        // 3. Save changes
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully deleted book with ID: {BookId}", bookId);

        return Result.Success;
    }
}