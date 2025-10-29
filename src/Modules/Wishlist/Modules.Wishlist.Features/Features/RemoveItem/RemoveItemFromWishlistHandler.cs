using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Wishlist.Domain.Errors;
using Modules.Wishlist.Infrastructure.Database;

namespace Modules.Wishlist.Features.Features.RemoveItem;

internal interface IRemoveItemFromWishlistHandler : IHandler
{
    Task<Result<Success>> HandleAsync(string userId, Guid bookId, CancellationToken ct);
}

internal sealed class RemoveItemFromWishlistHandler(
    WishlistDbContext dbContext,
    ILogger<RemoveItemFromWishlistHandler> logger)
    : IRemoveItemFromWishlistHandler
{
    public async Task<Result<Success>> HandleAsync(string userId, Guid bookId, CancellationToken ct)
    {
        logger.LogInformation("Removing Book {BookId} from wishlist for User {UserId}", bookId, userId);

        // Find the item
        var itemToRemove = await dbContext.WishlistItems
                                .FirstOrDefaultAsync(wi => wi.UserId == userId && wi.BookId == bookId, ct);

        if (itemToRemove is null)
        {
            logger.LogWarning("Remove from wishlist failed: Book {BookId} not in wishlist for User {UserId}", bookId, userId);
            return WishlistErrors.ItemNotFound(bookId);
        }

        // Remove and save
        dbContext.WishlistItems.Remove(itemToRemove);
        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation("Removed Book {BookId} from wishlist for User {UserId}", bookId, userId);
        return Result.Success;
    }
}