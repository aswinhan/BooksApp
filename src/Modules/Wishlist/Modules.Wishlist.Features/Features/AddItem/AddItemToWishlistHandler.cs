using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Catalog.PublicApi; // Need Catalog API
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Wishlist.Domain.Entities;
using Modules.Wishlist.Domain.Errors;
using Modules.Wishlist.Infrastructure.Database;

namespace Modules.Wishlist.Features.Features.AddItem;

internal interface IAddItemToWishlistHandler : IHandler
{
    Task<Result<Success>> HandleAsync(string userId, Guid bookId, CancellationToken ct);
}

internal sealed class AddItemToWishlistHandler(
    WishlistDbContext dbContext,
    ICatalogModuleApi catalogApi, // Inject Catalog API
    ILogger<AddItemToWishlistHandler> logger)
    : IAddItemToWishlistHandler
{
    public async Task<Result<Success>> HandleAsync(string userId, Guid bookId, CancellationToken ct)
    {
        logger.LogInformation("Adding Book {BookId} to wishlist for User {UserId}", bookId, userId);

        // 1. Check if item already exists
        bool itemExists = await dbContext.WishlistItems
                                .AnyAsync(wi => wi.UserId == userId && wi.BookId == bookId, ct);
        if (itemExists)
        {
            logger.LogWarning("Add to wishlist failed: Book {BookId} already in wishlist for User {UserId}", bookId, userId);
            return WishlistErrors.ItemAlreadyExists(bookId);
        }

        // 2. Check if book exists in catalog
        var bookResult = await catalogApi.GetBookByIdAsync(bookId, ct);
        if (bookResult.IsError)
        {
            logger.LogWarning("Add to wishlist failed: Book {BookId} not found in catalog.", bookId);
            return WishlistErrors.BookNotFound(bookId);
        }

        // 3. Create and add new item
        var wishlistItem = new WishlistItem(Guid.NewGuid(), userId, bookId);

        await dbContext.WishlistItems.AddAsync(wishlistItem, ct);
        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation("Added Book {BookId} to wishlist for User {UserId}", bookId, userId);
        return Result.Success;
    }
}