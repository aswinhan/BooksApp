using Modules.Common.Domain.Results;
using Modules.Inventory.Features.Features.CheckStock; // Use handler interfaces
using Modules.Inventory.Features.Features.DecreaseStock;
using Modules.Inventory.Features.Features.IncreaseStock;
using Modules.Inventory.Features.Features.GetStockLevel;
using Modules.Inventory.PublicApi; // Implement interface
using Modules.Inventory.PublicApi.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Inventory.Features.InternalApi;

internal sealed class InventoryModuleApi(
    ICheckStockHandler checkStockHandler,
    IDecreaseStockHandler decreaseStockHandler,
    IIncreaseStockHandler increaseStockHandler,
    IGetStockLevelHandler getStockLevelHandler)
    : IInventoryModuleApi
{
    public Task<Result<Success>> CheckStockAsync(StockAdjustmentRequest request, CancellationToken cancellationToken)
        => checkStockHandler.HandleAsync(request, cancellationToken);

    public Task<Result<Success>> DecreaseStockAsync(StockAdjustmentRequest request, CancellationToken cancellationToken)
        => decreaseStockHandler.HandleAsync(request, cancellationToken);

    public Task<Result<Success>> IncreaseStockAsync(StockAdjustmentRequest request, CancellationToken cancellationToken)
        => increaseStockHandler.HandleAsync(request, cancellationToken);

    public Task<Result<StockLevelDto>> GetStockLevelAsync(Guid bookId, CancellationToken cancellationToken)
         => getStockLevelHandler.HandleAsync(bookId, cancellationToken);
}