using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Orders.Infrastructure.Database;

namespace Modules.Orders.Features.DeliverOrder;

internal interface IDeliverOrderHandler : IHandler
{
    Task<Result<Success>> HandleAsync(Guid orderId, CancellationToken cancellationToken);
}

internal sealed class DeliverOrderHandler(
    OrdersDbContext dbContext,
    ILogger<DeliverOrderHandler> logger)
    : IDeliverOrderHandler
{
    public async Task<Result<Success>> HandleAsync(Guid orderId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to mark order {OrderId} as delivered", orderId);

        var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order is null)
        {
            logger.LogWarning("Deliver order failed: Order {OrderId} not found.", orderId);
            return Error.NotFound("Orders.NotFound", $"Order with ID {orderId} not found.");
        }

        // Execute Domain Logic
        var deliverResult = order.Deliver(); // Call domain method
        if (deliverResult.IsError)
        {
            logger.LogWarning("Deliver order failed for Order {OrderId}: {Reason}", orderId, deliverResult.FirstError.Description);
            return deliverResult.Errors!; // Return domain error
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        // Optional: Publish OrderDeliveredEvent
        // await eventPublisher.PublishAsync(new OrderDeliveredEvent(order.Id, order.UserId), cancellationToken);

        logger.LogInformation("Successfully marked order {OrderId} as delivered", orderId);
        return Result.Success;
    }
}