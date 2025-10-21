using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // For FirstOrDefaultAsync
using Microsoft.Extensions.Logging;
using Modules.Catalog.Domain.Entities; // For Book, Author
using Modules.Catalog.Features.Books.Shared.Responses;
using Modules.Catalog.Infrastructure.Database; // For CatalogDbContext
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results; // For Result<>, Error

namespace Modules.Catalog.Features.Books.CreateBook;

internal interface ICreateBookHandler : IHandler
{
    Task<Result<BookResponse>> HandleAsync(CreateBookRequest request, CancellationToken cancellationToken);
}

internal sealed class CreateBookHandler(
    CatalogDbContext dbContext, // Inject the module's DbContext
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
            request.AuthorId
        );

        // 3. Add to DbContext and Save
        await dbContext.Books.AddAsync(book, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully created book {BookId} with Title: {Title}", book.Id, book.Title);

        // 4. Map to Response DTO (explicit mapping)
        // We need AuthorName, but the book entity doesn't have it directly after creation.
        // For simplicity here, we'll fetch it again. In a real scenario, consider options.
        var author = await dbContext.Authors.FindAsync([request.AuthorId], cancellationToken: cancellationToken); // Find author for name


        var response = new BookResponse(
            book.Id,
            book.Title,
            book.Description,
            book.Isbn,
            book.Price,
            book.AuthorId,
            author?.Name ?? "Unknown Author", // Handle if author somehow not found
            [], // No reviews on creation
            book.CreatedAtUtc,
            book.UpdatedAtUtc
        );

        return response; // Implicit conversion to Result<BookResponse>
    }
}