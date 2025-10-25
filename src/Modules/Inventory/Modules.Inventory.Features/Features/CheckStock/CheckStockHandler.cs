using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Inventory.Domain.Entities; // Required for BookStock entity
using Modules.Inventory.Domain.Errors; // Required for InventoryErrors
using Modules.Inventory.Features.Features.Shared.Errors; // Required for ValidationExtensions
using Modules.Inventory.Infrastructure.Database;
using Modules.Inventory.PublicApi.Contracts; // Required for Request/Item DTOs
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Inventory.Features.Features.CheckStock;

internal interface ICheckStockHandler : IHandler
{
    Task<Result<Success>> HandleAsync(StockAdjustmentRequest request, CancellationToken cancellationToken);
}

internal sealed class CheckStockHandler(
    InventoryDbContext dbContext,
    IValidator<StockAdjustmentRequest> validator, // Inject validator
    ILogger<CheckStockHandler> logger)
    : ICheckStockHandler
{
    public async Task<Result<Success>> HandleAsync(StockAdjustmentRequest request, CancellationToken cancellationToken)
    {
        logger.LogDebug("Checking stock for {ItemCount} book types.", request.Items.Count);

        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            logger.LogWarning("Stock check validation failed.");
            return validationResult.ToDomainErrors();
        }

        // Fetch current stock levels for requested books
        var bookIds = request.Items.Select(x => x.BookId).ToList();
        var currentStocks = await dbContext.BookStocks
            .Where(x => bookIds.Contains(x.BookId))
            .ToDictionaryAsync(x => x.BookId, x => x.QuantityAvailable, cancellationToken);

        // Verify quantities
        var errors = new List<Error>();
        foreach (var item in request.Items)
        {
            if (!currentStocks.TryGetValue(item.BookId, out var availableQuantity))
            {
                errors.Add(InventoryErrors.BookNotFound(item.BookId));
            }
            else if (availableQuantity < item.Quantity) // Quantity in request is required amount
            {
                errors.Add(InventoryErrors.InsufficientStock(item.BookId, item.Quantity, availableQuantity));
            }
        }

        if (errors.Count > 0)
        {
            logger.LogWarning("Stock check failed due to insufficient stock or missing books.");
            return errors; // Return list of errors
        }

        logger.LogDebug("Stock check passed.");
        return Result.Success;
    }
}