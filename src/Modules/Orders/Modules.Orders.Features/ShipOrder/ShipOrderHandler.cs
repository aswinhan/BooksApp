using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Orders.Infrastructure.Database;

namespace Modules.Orders.Features.ShipOrder;

internal interface IShipOrderHandler : IHandler
{
    Task<Result<Success>> HandleAsync(Guid orderId, CancellationToken cancellationToken);
}

internal sealed class ShipOrderHandler(
    OrdersDbContext dbContext, // Inject DbContext to load/save aggregate
    ILogger<ShipOrderHandler> logger)
    : IShipOrderHandler
{
    public async Task<Result<Success>> HandleAsync(Guid orderId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to ship order {OrderId}", orderId);

        // 1. Find the Order Aggregate Root
        var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order is null)
        {
            logger.LogWarning("Ship order failed: Order {OrderId} not found.", orderId);
            return Error.NotFound("Orders.NotFound", $"Order with ID {orderId} not found.");
        }

        // Optional: Add authorization check here (e.g., ensure current user is Admin/Manager)

        // 2. Execute the Domain Logic via the Aggregate Root
        var shipResult = order.Ship(); // Call domain method
        if (shipResult.IsError)
        {
            logger.LogWarning("Ship order failed for Order {OrderId}: {Reason}", orderId, shipResult.FirstError.Description);
            // Return the domain error directly
            return shipResult.Errors!;
        }

        // 3. Save changes (EF Core tracks the status change)
        await dbContext.SaveChangesAsync(cancellationToken);

        // Optional: Publish an OrderShipped event
        // var orderShippedEvent = new OrderShippedEvent(order.Id, order.UserId);
        // await eventPublisher.PublishAsync(orderShippedEvent, cancellationToken);

        logger.LogInformation("Successfully shipped order {OrderId}", orderId);
        return Result.Success;
    }
}