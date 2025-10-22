using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Orders.Features.Shared.Responses; // Use OrderSummaryResponse
using Modules.Orders.Infrastructure.Database; // Use OrdersDbContext

namespace Modules.Orders.Features.GetMyOrders;

// Interface defines the contract
internal interface IGetMyOrdersHandler : IHandler
{
    // Adjusting return type for simplicity now
    Task<List<OrderSummaryResponse>> HandleAsync(string userId, CancellationToken cancellationToken);
}

// Implementation handles the query
internal sealed class GetMyOrdersHandler(
    OrdersDbContext dbContext, // Inject DbContext for reading
    ILogger<GetMyOrdersHandler> logger)
    : IGetMyOrdersHandler
{
    // Adjusting return type
    public async Task<List<OrderSummaryResponse>> HandleAsync(string userId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving orders for User {UserId}", userId);

        var orders = await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId) // Filter by the current user
            .Include(o => o.OrderItems) // Include items to get the count
            .OrderByDescending(o => o.CreatedAtUtc) // Show newest orders first
                                                    // Add Skip().Take() here for pagination later
            .Select(o => new OrderSummaryResponse( // Project directly to the DTO
                o.Id,
                o.CreatedAtUtc,
                o.Status,
                o.Total,
                o.OrderItems.Count // Get count from included items
            ))
            .ToListAsync(cancellationToken);

        logger.LogInformation("Retrieved {Count} orders for User {UserId}", orders.Count, userId);
        return orders;
    }
}