using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Orders.Infrastructure.Database;

namespace Modules.Orders.Features.CancelOrder;

internal interface ICancelOrderHandler : IHandler
{
    Task<Result<Success>> HandleAsync(Guid orderId, CancellationToken cancellationToken);
}

internal sealed class CancelOrderHandler(
    OrdersDbContext dbContext,
    ILogger<CancelOrderHandler> logger)
    // Inject IEventPublisher if cancelling needs to trigger stock increase
    // Inject IInventoryModuleApi if cancelling needs to trigger stock increase
    : ICancelOrderHandler
{
    public async Task<Result<Success>> HandleAsync(Guid orderId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to cancel order {OrderId}", orderId);

        var order = await dbContext.Orders
                            // Include OrderItems if needed for stock replenishment logic
                            // .Include(o => o.OrderItems)
                            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order is null)
        {
            logger.LogWarning("Cancel order failed: Order {OrderId} not found.", orderId);
            return Error.NotFound("Orders.NotFound", $"Order with ID {orderId} not found.");
        }

        // Optional: Authorization Check (e.g., Is user the owner or an admin?)

        // Execute Domain Logic
        var cancelResult = order.Cancel(); // Call domain method
        if (cancelResult.IsError)
        {
            logger.LogWarning("Cancel order failed for Order {OrderId}: {Reason}", orderId, cancelResult.FirstError.Description);
            return cancelResult.Errors!; // Return domain error
        }

        // Important: If Cancel() succeeded but status didn't *change* (was already Cancelled),
        // we might not need to save or publish events.
        // Check if status actually changed before proceeding.
        bool statusChanged = dbContext.Entry(order).Property(o => o.Status).IsModified;


        if (statusChanged)
        {
            await dbContext.SaveChangesAsync(cancellationToken);

            // --- IMPORTANT: Handle Side Effects ---
            // If cancelling requires increasing stock, do it here.
            // Option 1: Publish OrderCancelledEvent and handle stock in Inventory module.
            // Option 2: Directly call IInventoryModuleApi.IncreaseStockAsync(...)
            // Example (Option 1 - preferred for loose coupling):
            // var cancelledEvent = new OrderCancelledEvent(order.Id, order.OrderItems.Select(...));
            // await eventPublisher.PublishAsync(cancelledEvent, cancellationToken);

            logger.LogInformation("Successfully cancelled order {OrderId}", orderId);
        }
        else
        {
            logger.LogInformation("Order {OrderId} was already cancelled. No action taken.", orderId);
        }


        return Result.Success;
    }
}