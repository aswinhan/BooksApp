using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Modules.Catalog.PublicApi; // To get book details
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Orders.Domain.Abstractions; // ICartService
using Modules.Orders.Domain.DTOs; // CartItemDto

namespace Modules.Orders.Features.Cart.AddItem;

internal interface IAddItemHandler : IHandler
{
    Task<Result<Success>> HandleAsync(string userId, AddItemRequest request, CancellationToken cancellationToken);
}

internal sealed class AddItemHandler(
    ICartService cartService,
    ICatalogModuleApi catalogApi, // Inject Catalog API
    ILogger<AddItemHandler> logger)
    : IAddItemHandler
{
    public async Task<Result<Success>> HandleAsync(
        string userId,
        AddItemRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to add Book {BookId} (Qty: {Quantity}) to cart for User {UserId}",
            request.BookId, request.Quantity, userId);

        // 1. Get Book Details from Catalog Module
        var bookResult = await catalogApi.GetBookByIdAsync(request.BookId, cancellationToken);
        if (bookResult.IsError)
        {
            logger.LogWarning("Add item failed: Book {BookId} not found.", request.BookId);
            // Return the error from the Catalog module
            return bookResult.Errors!;
        }
        var bookDetails = bookResult.Value!;

        // 2. Create CartItem DTO
        var cartItem = new CartItemDto
        {
            BookId = bookDetails.Id,
            Title = bookDetails.Title,
            Price = bookDetails.Price,
            Quantity = request.Quantity
        };

        // 3. Add to Cart via Service
        await cartService.AddItemToCartAsync(userId, cartItem);

        logger.LogInformation("Successfully added Book {BookId} to cart for User {UserId}", request.BookId, userId);
        return Result.Success;
    }
}