using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Orders.Domain.Enums; // Need OrderStatus
using Modules.Orders.Infrastructure.Database;
using Modules.Orders.PublicApi.Contracts; // Use DTOs

namespace Modules.Orders.Features.CheckPurchase;

internal interface ICheckPurchaseHandler : IHandler
{
    Task<Result<CheckPurchaseResponse>> HandleAsync(CheckPurchaseRequest request, CancellationToken cancellationToken);
}

internal sealed class CheckPurchaseHandler(
    OrdersDbContext dbContext,
    ILogger<CheckPurchaseHandler> logger) : ICheckPurchaseHandler
{
    public async Task<Result<CheckPurchaseResponse>> HandleAsync(CheckPurchaseRequest request, CancellationToken cancellationToken)
    {
        logger.LogDebug("Checking purchase for User {UserId}, Book {BookId}", request.UserId, request.BookId);

        // Check if there's any COMPLETED order item matching the user and book
        // Define "completed" status appropriately (e.g., Shipped or Delivered)
        bool hasPurchased = await dbContext.OrderItems
            .AnyAsync(oi => oi.BookId == request.BookId &&
                            oi.Order.UserId == request.UserId &&
                            (oi.Order.Status == OrderStatus.Shipped || oi.Order.Status == OrderStatus.Delivered),
                     cancellationToken);

        logger.LogDebug("Purchase check result for User {UserId}, Book {BookId}: {HasPurchased}",
            request.UserId, request.BookId, hasPurchased);

        return new CheckPurchaseResponse(hasPurchased); // Implicit success
    }
}