using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Inventory.Domain.Entities;
using Modules.Inventory.Domain.Errors;
using Modules.Inventory.Features.Features.Shared.Errors;
using Modules.Inventory.Infrastructure.Database;
using Modules.Inventory.PublicApi.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Inventory.Features.Features.DecreaseStock;

internal interface IDecreaseStockHandler : IHandler
{
    Task<Result<Success>> HandleAsync(StockAdjustmentRequest request, CancellationToken cancellationToken);
}

internal sealed class DecreaseStockHandler(
    InventoryDbContext dbContext,
    IValidator<StockAdjustmentRequest> validator, // Reuse validator
    ILogger<DecreaseStockHandler> logger)
    : IDecreaseStockHandler
{
    public async Task<Result<Success>> HandleAsync(StockAdjustmentRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to decrease stock for {ItemCount} book types.", request.Items.Count);

        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            logger.LogWarning("Decrease stock validation failed.");
            return validationResult.ToDomainErrors();
        }

        // Use transaction to ensure all stock updates succeed or fail together
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var bookIds = request.Items.Select(x => x.BookId).ToList();
            // Lock the rows for update
            var currentStocks = await dbContext.BookStocks
                .Where(x => bookIds.Contains(x.BookId))
                // Consider adding .SetLockingBehavior(LockingBehavior.PessimisticUpdate) or similar
                // depending on your specific concurrency needs and database provider (requires EF Core >= 7)
                .ToDictionaryAsync(x => x.BookId, x => x, cancellationToken);

            var errors = new List<Error>();
            foreach (var item in request.Items)
            {
                if (!currentStocks.TryGetValue(item.BookId, out var stockRecord))
                {
                    errors.Add(InventoryErrors.BookNotFound(item.BookId));
                    continue; // Cannot proceed if book not found
                }

                // Use domain entity logic to decrease stock
                var decreaseResult = stockRecord.DecreaseStock(item.Quantity);
                if (decreaseResult.IsError)
                {
                    errors.AddRange(decreaseResult.Errors!);
                }
            }

            if (errors.Count > 0)
            {
                logger.LogWarning("Decrease stock failed due to insufficient stock or missing books.");
                await transaction.RollbackAsync(cancellationToken);
                return errors; // Return list of errors
            }

            // If all checks passed, save changes and commit
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            logger.LogInformation("Successfully decreased stock.");
            return Result.Success;
        }
        catch (Exception ex) // Catch DbUpdateConcurrencyException etc.
        {
            logger.LogError(ex, "Error occurred during stock decrease transaction.");
            await transaction.RollbackAsync(cancellationToken);
            return Error.Unexpected("Inventory.DecreaseFailed", "Failed to decrease stock due to a server error.");
        }
    }
}