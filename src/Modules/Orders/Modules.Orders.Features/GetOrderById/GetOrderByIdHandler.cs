using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Orders.Features.Shared.Responses;
using Modules.Orders.Infrastructure.Database;

namespace Modules.Orders.Features.GetOrderById;

internal interface IGetOrderByIdHandler : IHandler
{
    Task<Result<OrderResponse>> HandleAsync(Guid orderId, string userId, bool isAdmin, CancellationToken cancellationToken);
}

internal sealed class GetOrderByIdHandler(
    OrdersDbContext dbContext,
    ILogger<GetOrderByIdHandler> logger)
    : IGetOrderByIdHandler
{
    public async Task<Result<OrderResponse>> HandleAsync(Guid orderId, string userId, bool isAdmin, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to retrieve order {OrderId} for User {UserId}", orderId, userId);

        var order = await dbContext.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems) // Eager load items
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order is null)
        {
            logger.LogWarning("Order {OrderId} not found.", orderId);
            return Error.NotFound("Orders.NotFound", $"Order with ID {orderId} not found.");
        }

        // --- Authorization Check ---
        // Allow access only if the current user owns the order OR is an admin
        if (order.UserId != userId && !isAdmin)
        {
            logger.LogWarning("User {UserId} forbidden from accessing Order {OrderId}.", userId, orderId);
            return Error.Forbidden("Orders.AccessDenied", "User is not authorized to view this order.");
        }

        // Map to response DTO
        var response = new OrderResponse(
            order.Id,
            order.UserId,
            order.ShippingAddress,
            order.Status,
            order.Total,
            order.OrderItems.Select(oi => new OrderItemResponse(
                oi.Id,
                oi.BookId,
                oi.BookTitle,
                oi.Price,
                oi.Quantity
            )).ToList(),
            order.CreatedAtUtc,
            order.UpdatedAtUtc
        );

        logger.LogInformation("Successfully retrieved order {OrderId} for User {UserId}", orderId, userId);
        return response;
    }
}