using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Inventory.Domain.Errors;
using Modules.Inventory.Infrastructure.Database;
using Modules.Inventory.PublicApi.Contracts; // Need StockLevelDto
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Inventory.Features.Features.GetStockLevel;

internal interface IGetStockLevelHandler : IHandler
{
    Task<Result<StockLevelDto>> HandleAsync(Guid bookId, CancellationToken cancellationToken);
}

internal sealed class GetStockLevelHandler(
    InventoryDbContext dbContext,
    ILogger<GetStockLevelHandler> logger)
    : IGetStockLevelHandler
{
    public async Task<Result<StockLevelDto>> HandleAsync(Guid bookId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Getting stock level for Book {BookId}", bookId);

        var stockRecord = await dbContext.BookStocks
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(bs => bs.BookId == bookId, cancellationToken);

        if (stockRecord == null)
        {
            // Decide: Return NotFound error or return 0 quantity?
            // Returning 0 quantity might be simpler for callers.
            logger.LogWarning("Stock record for Book {BookId} not found, returning 0.", bookId);
            return new StockLevelDto(bookId, 0);
            // Or: return InventoryErrors.BookNotFound(bookId);
        }

        return new StockLevelDto(stockRecord.BookId, stockRecord.QuantityAvailable);
    }
}