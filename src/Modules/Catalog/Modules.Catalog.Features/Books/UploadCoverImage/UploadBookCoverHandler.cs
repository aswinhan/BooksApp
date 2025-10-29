using System;
using System.IO; // For Path
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http; // For IFormFile
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Catalog.Features.Books.Shared.Responses;
using Modules.Catalog.Infrastructure.Database;
using Modules.Common.Application.Storage; // Use IFileStorageService
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;

namespace Modules.Catalog.Features.Books.UploadCoverImage;

internal interface IUploadBookCoverHandler : IHandler
{
    Task<Result<BookResponse>> HandleAsync(Guid bookId, IFormFile file, CancellationToken cancellationToken);
}

internal sealed class UploadBookCoverHandler(
    CatalogDbContext dbContext,
    IFileStorageService fileStorageService, // <-- Inject storage service
    ILogger<UploadBookCoverHandler> logger)
    : IUploadBookCoverHandler
{
    public async Task<Result<BookResponse>> HandleAsync(Guid bookId, IFormFile file, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to upload cover image for Book {BookId}", bookId);

        var book = await dbContext.Books
            .Include(b => b.Author) // Include data needed for response
            .Include(b => b.Reviews)
            .FirstOrDefaultAsync(b => b.Id == bookId, cancellationToken);

        if (book is null)
        {
            logger.LogWarning("Upload cover failed: Book {BookId} not found.", bookId);
            return Error.NotFound("Catalog.BookNotFound", $"Book with ID {bookId} not found.");
        }

        // --- Upload File ---
        string newFileUrl;
        try
        {
            // Define container and potentially filename
            string container = "book-covers";
            // Create a unique filename based on BookId and extension
            string blobName = $"{bookId}{Path.GetExtension(file.FileName)}";

            // If a previous image exists, delete it first
            if (!string.IsNullOrEmpty(book.CoverImageUrl))
            {
                await fileStorageService.DeleteFileAsync(book.CoverImageUrl, cancellationToken);
            }

            // Save the new file
            newFileUrl = await fileStorageService.SaveFileAsync(file, container, blobName, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save cover image file for Book {BookId}", bookId);
            return Error.Unexpected("Catalog.FileUploadFailed", "Failed to save cover image.");
        }
        // --- End Upload File ---

        // --- Update Book Entity ---
        book.SetCoverImage(newFileUrl); // Use domain method to set URL
        // --- End Update ---

        // Save changes to the database
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully uploaded cover image for Book {BookId}. New URL: {ImageUrl}", bookId, newFileUrl);

        // Map updated entity to Response DTO (including new URL and existing stats)
        int reviewCount = book.Reviews.Count;
        double? averageRating = reviewCount > 0 ? book.Reviews.Average(r => r.Rating.Value) : null;
        int quantityAvailable = 0; // Fetch from Inventory if needed

        var response = new BookResponse(
             book.Id, book.Title, book.Description, book.Isbn, book.Price,
             book.AuthorId, book.Author.Name,
             book.Reviews.Select(r => new ReviewResponse(r.Id, r.UserId, r.Comment, r.Rating.Value, r.CreatedAtUtc)).ToList(),
             quantityAvailable, // Pass quantity
             book.CoverImageUrl, // Pass new cover image URL
             averageRating,
             reviewCount,
             book.CreatedAtUtc, book.UpdatedAtUtc // UpdatedAtUtc will be set
         );

        return response;
    }
}