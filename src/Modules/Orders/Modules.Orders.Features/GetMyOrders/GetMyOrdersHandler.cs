using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Common.Application.Pagination;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Orders.Features.Shared.Responses; // Use OrderSummaryResponse
using Modules.Orders.Infrastructure.Database; // Use OrdersDbContext
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Orders.Features.GetMyOrders;

// Interface defines the contract
// Add pageNumber, pageSize parameters and change return type
internal interface IGetMyOrdersHandler : IHandler
{
    Task<Result<PaginatedResponse<OrderSummaryResponse>>> HandleAsync(
        string userId, int pageNumber, int pageSize, CancellationToken cancellationToken);
}

// Implementation handles the query
internal sealed class GetMyOrdersHandler(
    OrdersDbContext dbContext, // Inject DbContext for reading
    ILogger<GetMyOrdersHandler> logger)
    : IGetMyOrdersHandler
{
    public async Task<Result<PaginatedResponse<OrderSummaryResponse>>> HandleAsync(
    string userId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving orders for User {UserId} - Page: {PageNumber}, Size: {PageSize}", userId, pageNumber, pageSize);

        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 5, 50);

        try
        {
            var query = dbContext.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId) // Keep filter
                .Include(o => o.OrderItems) // Keep include for count
                .OrderByDescending(o => o.CreatedAtUtc); // Keep sort

            int totalCount = await query.CountAsync(cancellationToken);

            var orders = await query
                .Skip((pageNumber - 1) * pageSize) // Apply pagination
                .Take(pageSize) // Apply pagination
                .Select(o => new OrderSummaryResponse(
                    o.Id, o.CreatedAtUtc, o.Status, o.Total, o.OrderItems.Count
                ))
                .ToListAsync(cancellationToken);

            logger.LogInformation("Retrieved {Count} orders for User {UserId}, page {PageNumber}.", orders.Count, userId, pageNumber);

            var paginatedResponse = new PaginatedResponse<OrderSummaryResponse>(
                orders, totalCount, pageNumber, pageSize
            );

            return paginatedResponse;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve orders list for User {UserId}, page {PageNumber}.", userId, pageNumber);
            return Error.Unexpected("Orders.GetMyListFailed", "Failed to retrieve order list.");
        }
    }
}