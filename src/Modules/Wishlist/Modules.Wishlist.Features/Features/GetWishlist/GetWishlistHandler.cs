using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Catalog.PublicApi; // Need Catalog API
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Inventory.PublicApi; // Need Inventory API
using Modules.Wishlist.Features.Features.Shared.Responses;
using Modules.Wishlist.Infrastructure.Database;

namespace Modules.Wishlist.Features.Features.GetWishlist;

internal interface IGetWishlistHandler : IHandler
{
    Task<Result<List<WishlistItemResponse>>> HandleAsync(string userId, CancellationToken ct);
}

internal sealed class GetWishlistHandler(
    WishlistDbContext dbContext,
    ICatalogModuleApi catalogApi, // Inject Catalog API
    IInventoryModuleApi inventoryApi, // Inject Inventory API
    ILogger<GetWishlistHandler> logger)
    : IGetWishlistHandler
{
    public async Task<Result<List<WishlistItemResponse>>> HandleAsync(string userId, CancellationToken ct)
    {
        logger.LogInformation("Getting wishlist for User {UserId}", userId);
        try
        {
            var wishlistItems = await dbContext.WishlistItems
                .AsNoTracking()
                .Where(wi => wi.UserId == userId)
                .OrderByDescending(wi => wi.CreatedAtUtc)
                .ToListAsync(ct);

            if (wishlistItems.Count == 0)
            {
                return new List<WishlistItemResponse>(); // Return empty list
            }

            var responseList = new List<WishlistItemResponse>();
            foreach (var item in wishlistItems)
            {
                // Call Catalog API to get book details
                var bookResult = await catalogApi.GetBookByIdAsync(item.BookId, ct);
                if (bookResult.IsError)
                {
                    logger.LogWarning("Could not find book details for BookId {BookId} in wishlist.", item.BookId);
                    continue; // Skip item if book not found
                }
                var book = bookResult.Value!;

                // Call Inventory API to get stock status
                var stockResult = await inventoryApi.GetStockLevelAsync(item.BookId, ct);
                int stock = stockResult.IsSuccess ? stockResult.Value!.QuantityAvailable : 0;

                responseList.Add(new WishlistItemResponse(
                    item.BookId,
                    book.Title,
                    book.AuthorName,
                    book.Price,
                    book.CoverImageUrl,
                    stock,
                    item.CreatedAtUtc
                ));
            }

            logger.LogInformation("Retrieved {Count} items for User {UserId} wishlist.", responseList.Count, userId);
            return responseList;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed retrieving wishlist for User {UserId}", userId);
            return Error.Unexpected("Wishlist.GetFailed", "Failed to retrieve wishlist.");
        }
    }
}