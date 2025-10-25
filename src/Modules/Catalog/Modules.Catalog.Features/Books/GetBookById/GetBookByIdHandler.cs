using System;
using System.Linq; // For Select
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // For Include, FirstOrDefaultAsync, AsNoTracking
using Microsoft.Extensions.Logging;
using Modules.Catalog.Domain.Entities; // For Book
using Modules.Catalog.Features.Books.Shared.Responses;
using Modules.Catalog.Infrastructure.Database; // For CatalogDbContext
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results; // For Result<>, Error
using Modules.Inventory.PublicApi;

namespace Modules.Catalog.Features.Books.GetBookById;

// Interface for the handler
internal interface IGetBookByIdHandler : IHandler
{
    Task<Result<BookResponse>> HandleAsync(Guid bookId, CancellationToken cancellationToken);
}

// Implementation of the handler
internal sealed class GetBookByIdHandler(
    CatalogDbContext dbContext, // Inject the DbContext for querying
    IInventoryModuleApi inventoryApi,
    ILogger<GetBookByIdHandler> logger)
    : IGetBookByIdHandler
{
    public async Task<Result<BookResponse>> HandleAsync(
        Guid bookId,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to retrieve book with ID: {BookId}", bookId);

        // Query the database, including related Author and Reviews
        // Use AsNoTracking() for read-only queries for better performance
        var book = await dbContext.Books
            .AsNoTracking()
            .Include(b => b.Author) // Eager load the Author
            .Include(b => b.Reviews) // Eager load the Reviews
            .FirstOrDefaultAsync(b => b.Id == bookId, cancellationToken);

        if (book is null)
        {
            logger.LogWarning("Book with ID {BookId} not found.", bookId);
            return Error.NotFound("Catalog.BookNotFound", $"Book with ID {bookId} not found.");
        }

        // --- Get Stock Level ---
        int quantityAvailable = 0; // Default to 0
        var stockResult = await inventoryApi.GetStockLevelAsync(bookId, cancellationToken);
        if (stockResult.IsSuccess && stockResult.Value != null)
        {
            quantityAvailable = stockResult.Value.QuantityAvailable;
        }
        else if (stockResult.IsError)
        {
            // Log warning but continue, maybe stock record doesn't exist yet
            logger.LogWarning("Could not retrieve stock level for Book {BookId}: {Error}", bookId, stockResult.FirstError.Code);
        }
        // --- End Get Stock Level ---

        // Map the Book entity and its related data to the response DTO
        var response = new BookResponse(
            book.Id,
            book.Title,
            book.Description,
            book.Isbn,
            book.Price,
            book.AuthorId,
            book.Author.Name, // Get name from loaded Author
            book.Reviews.Select(r => new ReviewResponse( // Map reviews
                r.Id,
                r.UserId,
                r.Comment,
                r.Rating.Value, // Get value from Rating VO
                r.CreatedAtUtc
            )).ToList(),
            quantityAvailable,
            book.CreatedAtUtc,
            book.UpdatedAtUtc
        );

        logger.LogInformation("Successfully retrieved book with ID: {BookId}", bookId);
        return response; // Implicit conversion to Result<BookResponse>
    }
}