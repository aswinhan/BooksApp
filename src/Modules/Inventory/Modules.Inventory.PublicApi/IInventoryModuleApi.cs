using Modules.Common.Domain.Results;
using Modules.Inventory.PublicApi.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Inventory.PublicApi;

public interface IInventoryModuleApi
{
    // Checks if enough stock is available (can return multiple errors)
    Task<Result<Success>> CheckStockAsync(StockAdjustmentRequest request, CancellationToken cancellationToken);

    // Decreases stock (transactional within module)
    Task<Result<Success>> DecreaseStockAsync(StockAdjustmentRequest request, CancellationToken cancellationToken);

    // Increases stock (transactional within module)
    Task<Result<Success>> IncreaseStockAsync(StockAdjustmentRequest request, CancellationToken cancellationToken);

    // Gets current stock level for a book
    Task<Result<StockLevelDto>> GetStockLevelAsync(Guid bookId, CancellationToken cancellationToken);
}