// Inside Modules.Inventory.Features.InternalApi.Decorators
using Modules.Common.Domain.Results;
using Modules.Inventory.Features.Tracing; // Use InventoryActivitySource
using Modules.Inventory.PublicApi;
using Modules.Inventory.PublicApi.Contracts;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Inventory.Features.InternalApi.Decorators;

public class TracedInventoryModuleApi(IInventoryModuleApi inner) : IInventoryModuleApi
{
    // Implement CheckStockAsync, DecreaseStockAsync, IncreaseStockAsync, GetStockLevelAsync
    // Wrap calls to 'inner' with activity spans using InventoryActivitySource.Instance

    public async Task<Result<Success>> CheckStockAsync(StockAdjustmentRequest request, CancellationToken cancellationToken)
    {
        using var activity = InventoryActivitySource.Instance.StartActivity($"{InventoryActivitySource.ActivitySourceName}.check-stock");
        // Add relevant tags...
        try { var response = await inner.CheckStockAsync(request, cancellationToken); activity?.SetStatus(ActivityStatusCode.Ok); return response; }
        catch (Exception ex) { activity?.SetStatus(ActivityStatusCode.Error, ex.Message); throw; }
    }

    public async Task<Result<Success>> DecreaseStockAsync(StockAdjustmentRequest request, CancellationToken cancellationToken)
    {
        using var activity = InventoryActivitySource.Instance.StartActivity($"{InventoryActivitySource.ActivitySourceName}.decrease-stock");
        // Add relevant tags...
        try { var response = await inner.DecreaseStockAsync(request, cancellationToken); activity?.SetStatus(ActivityStatusCode.Ok); return response; }
        catch (Exception ex) { activity?.SetStatus(ActivityStatusCode.Error, ex.Message); throw; }
    }

    public async Task<Result<Success>> IncreaseStockAsync(StockAdjustmentRequest request, CancellationToken cancellationToken)
    {
        using var activity = InventoryActivitySource.Instance.StartActivity($"{InventoryActivitySource.ActivitySourceName}.increase-stock");
        // Add relevant tags...
        try { var response = await inner.IncreaseStockAsync(request, cancellationToken); activity?.SetStatus(ActivityStatusCode.Ok); return response; }
        catch (Exception ex) { activity?.SetStatus(ActivityStatusCode.Error, ex.Message); throw; }
    }

    public async Task<Result<StockLevelDto>> GetStockLevelAsync(Guid bookId, CancellationToken cancellationToken)
    {
        using var activity = InventoryActivitySource.Instance.StartActivity($"{InventoryActivitySource.ActivitySourceName}.get-stock");
        // Add relevant tags...
        try { var response = await inner.GetStockLevelAsync(bookId, cancellationToken); activity?.SetStatus(ActivityStatusCode.Ok); return response; }
        catch (Exception ex) { activity?.SetStatus(ActivityStatusCode.Error, ex.Message); throw; }
    }
}