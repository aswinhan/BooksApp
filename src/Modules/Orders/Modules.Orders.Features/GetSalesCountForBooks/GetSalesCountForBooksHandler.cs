using System; // Added for Exception
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Orders.Domain.Enums;
using Modules.Orders.Infrastructure.Database;
using Modules.Orders.PublicApi.Contracts; // <-- ADD THIS USING STATEMENT

namespace Modules.Orders.Features.GetSalesCountForBooks;

// Interface Definition (ensure parameters match implementation)
internal interface IGetSalesCountForBooksHandler : IHandler
{
    // Ensure this signature matches the implementation below
    Task<Result<int>> HandleAsync(GetSalesCountForBooksRequest request, CancellationToken cancellationToken);
}

// Implementation
internal sealed class GetSalesCountForBooksHandler(
    OrdersDbContext dbContext,
    ILogger<GetSalesCountForBooksHandler> logger) : IGetSalesCountForBooksHandler
{
    // Ensure this signature matches the interface above
    public async Task<Result<int>> HandleAsync(GetSalesCountForBooksRequest request, CancellationToken cancellationToken)
    {
        if (request.BookIds == null || request.BookIds.Count == 0)
        {
            return 0; // Return 0 if no book IDs are provided
        }

        logger.LogDebug("Calculating total sales for {BookCount} book IDs.", request.BookIds.Count);

        try
        {
            var query = dbContext.OrderItems
                .AsNoTracking()
                .Where(oi => request.BookIds.Contains(oi.BookId) &&
                             (oi.Order.Status == OrderStatus.Shipped || oi.Order.Status == OrderStatus.Delivered));

            int totalSales = await query.SumAsync(oi => oi.Quantity, cancellationToken);

            logger.LogDebug("Total sales found: {TotalSales}", totalSales);
            return totalSales; // Implicit conversion
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to calculate sales count for book IDs.");
            return Error.Unexpected("Orders.SalesCountFailed", "Failed to calculate sales count.");
        }
    }
}