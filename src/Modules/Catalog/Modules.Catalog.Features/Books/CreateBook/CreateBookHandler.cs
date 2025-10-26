using Microsoft.EntityFrameworkCore; // For FirstOrDefaultAsync
using Microsoft.Extensions.Logging;
using Modules.Catalog.Domain.Entities; // For Book, Author
using Modules.Catalog.Features.Books.Shared.Responses;
using Modules.Catalog.Infrastructure.Database; // For CatalogDbContext
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results; // For Result<>, Error
using Modules.Inventory.PublicApi;
using Modules.Inventory.PublicApi.Contracts;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Catalog.Features.Books.CreateBook;

internal interface ICreateBookHandler : IHandler
{
    Task<Result<BookResponse>> HandleAsync(CreateBookRequest request, CancellationToken cancellationToken);
}

internal sealed class CreateBookHandler(
    CatalogDbContext dbContext, // Inject the module's DbContext
    IInventoryModuleApi inventoryApi,
    ILogger<CreateBookHandler> logger)
    : ICreateBookHandler
{
    public async Task<Result<BookResponse>> HandleAsync(
        CreateBookRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to create book: {Title}", request.Title);

        // 1. Verify Author Exists
        var authorExists = await dbContext.Authors
                                     .AnyAsync(a => a.Id == request.AuthorId, cancellationToken);
        if (!authorExists)
        {
            logger.LogWarning("Create book failed: Author {AuthorId} not found.", request.AuthorId);
            return Error.NotFound("Catalog.AuthorNotFound", $"Author with ID {request.AuthorId} not found.");
        }

        // Optional: Check if book with same ISBN already exists
        var isbnExists = await dbContext.Books.AnyAsync(b => b.Isbn == request.Isbn, cancellationToken);
        if (isbnExists)
        {
            logger.LogWarning("Create book failed: ISBN {ISBN} already exists.", request.Isbn);
            return Error.Conflict("Catalog.IsbnExists", $"Book with ISBN {request.Isbn} already exists.");
        }


        // 2. Create the Book entity using its constructor
        var book = new Book(
            Guid.NewGuid(),
            request.Title,
            request.Description,
            request.Isbn,
            request.Price,
            request.AuthorId,
            request.CategoryId
        );

        // 3. Add to DbContext and Save
        await dbContext.Books.AddAsync(book, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully created book {BookId} with Title: {Title}", book.Id, book.Title);

        // --- Create Initial Stock Record ---
        try
        {
            // Use IncreaseStock with initial quantity 0 (or a default starting value)
            var stockRequest = new StockAdjustmentRequest(
                [new StockAdjustmentItem(book.Id, 0)] // Create with 0 quantity initially
            );
            // Note: We use IncreaseStock(0) as a way to ensure the record exists.
            // The SetStock feature is for admin manual override.
            // A dedicated "InitializeStock" method in IInventoryModuleApi might be cleaner.
            // Let's adapt SetStockHandler logic slightly to handle creation on first set.

            // --- Alternative: Using SetStock feature internally ---
            // Requires adapting SetStockHandler to be callable internally or adding a new internal handler.
            // For now, let's assume we need an admin to set the initial stock via the SetStock endpoint.
            // We won't automatically create the stock record here yet.
            // We'll rely on GetStockLevelAsync to return 0 if the record doesn't exist.
            logger.LogInformation("Inventory record for Book {BookId} should be created manually via admin endpoint.", book.Id);

            /* --- If automatically creating stock ---
            // We need an "EnsureStockRecordExists" or similar internal method.
            // For now, assume GetStockLevelAsync handles non-existence gracefully.
            */
        }
        catch (Exception ex)
        {
            // Log error if creating initial stock fails, but don't fail the whole operation
            logger.LogError(ex, "Failed to ensure initial stock record for Book {BookId}", book.Id);
        }
        // --- End Initial Stock ---

        // 4. Map to Response DTO (explicit mapping)
        // We need AuthorName, but the book entity doesn't have it directly after creation.
        // For simplicity here, we'll fetch it again. In a real scenario, consider options.
        var author = await dbContext.Authors.FindAsync([request.AuthorId], cancellationToken: cancellationToken); // Find author for name


        // Ensure ALL parameters match the BookResponse record definition IN ORDER
        var response = new BookResponse(
            book.Id,
            book.Title,
            book.Description,
            book.Isbn,
            book.Price,
            book.AuthorId,
            author?.Name ?? "Unknown Author",
            [], // New book has no reviews yet
            0,  // Initial stock quantity is 0 (or fetch if created)
            book.CoverImageUrl, // Pass cover image URL (likely null on create)
            null, // New book has no average rating yet
            0,    // New book has 0 reviews yet
            book.CreatedAtUtc, // Pass CreatedAtUtc
            book.UpdatedAtUtc  // Pass UpdatedAtUtc (will be null on create)
        );

        return response; // Implicit conversion to Result<BookResponse>
    }
}