using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // Required for transaction
using Microsoft.Extensions.Logging;
using Modules.Catalog.PublicApi; // Required to get book details
using Modules.Common.Domain.Events; // Required for IEventPublisher
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Orders.Domain.Abstractions; // Required for ICartService
using Modules.Orders.Domain.Entities; // Required for Order, OrderItem
using Modules.Orders.Infrastructure.Database; // Required for OrdersDbContext

namespace Modules.Orders.Features.Checkout;

// Define an OrderCreated event (can go in Domain/Events if preferred)
public record OrderCreatedEvent(Guid OrderId, string UserId) : IEvent;

internal interface ICheckoutHandler : IHandler
{
    Task<Result<Guid>> HandleAsync(string userId, CheckoutRequest request, CancellationToken cancellationToken);
}

internal sealed class CheckoutHandler(
    ICartService cartService,
    ICatalogModuleApi catalogApi,
    OrdersDbContext dbContext, // Inject DbContext for aggregate operations & transaction
    IEventPublisher eventPublisher, // Inject publisher for events
    ILogger<CheckoutHandler> logger)
    : ICheckoutHandler
{
    public async Task<Result<Guid>> HandleAsync(
        string userId,
        CheckoutRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting checkout for User {UserId}", userId);

        // 1. Get Cart
        var cart = await cartService.GetCartAsync(userId);
        if (cart.Items.Count == 0)
        {
            logger.LogWarning("Checkout failed for User {UserId}: Cart is empty.", userId);
            return Error.Validation("Orders.CartEmpty", "Cannot checkout with an empty cart.");
        }

        // --- Begin Transaction ---
        // Use a transaction to ensure Order creation and Cart clearing are atomic
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // 2. Create Order Aggregate Root
            var order = new Order(
                Guid.NewGuid(),
                userId,
                request.ShippingAddress // Already validated by endpoint validator
            );

            // 3. Add OrderItems (Snapshotting product details)
            foreach (var item in cart.Items)
            {
                // Re-verify book details *within* the transaction for consistency? Optional.
                // var bookResult = await catalogApi.GetBookByIdAsync(item.BookId, cancellationToken);
                // if (bookResult.IsError) { /* Handle error */ }
                // var currentPrice = bookResult.Value.Price;
                // var currentTitle = bookResult.Value.Title;

                // Use details already stored in the cart DTO
                order.AddOrderItem(item.BookId, item.Title, item.Price, item.Quantity);
            }

            // 4. Add Order to DbContext
            await dbContext.Orders.AddAsync(order, cancellationToken);

            // 5. Save Order to Database (part of transaction)
            await dbContext.SaveChangesAsync(cancellationToken);

            // 6. Clear Cart (only after successful save)
            await cartService.ClearCartAsync(userId);

            // --- Commit Transaction ---
            await transaction.CommitAsync(cancellationToken);

            logger.LogInformation("Checkout successful for User {UserId}. Order {OrderId} created.", userId, order.Id);

            // 7. Publish OrderCreated Event (after successful commit)
            var orderEvent = new OrderCreatedEvent(order.Id, userId);
            await eventPublisher.PublishAsync(orderEvent, cancellationToken);


            // 8. Return the new Order ID
            return order.Id; // Implicit conversion to Result<Guid>
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Checkout failed for User {UserId} during transaction.", userId);
            await transaction.RollbackAsync(cancellationToken); // Rollback on error
            // Return a generic error
            return Error.Unexpected("Orders.CheckoutFailed", "An unexpected error occurred during checkout.");
        }
    }
}