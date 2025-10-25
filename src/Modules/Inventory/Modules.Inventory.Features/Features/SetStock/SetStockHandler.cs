using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Inventory.Domain.Entities;
using Modules.Inventory.Domain.Errors;
using Modules.Inventory.Infrastructure.Database;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Inventory.Features.Features.SetStock;

internal interface ISetStockHandler : IHandler { Task<Result<Success>> HandleAsync(SetStockRequest request, CancellationToken ct); }
internal sealed class SetStockHandler(InventoryDbContext db, ILogger<SetStockHandler> l) : ISetStockHandler
{
    public async Task<Result<Success>> HandleAsync(SetStockRequest request, CancellationToken ct)
    {
        l.LogInformation("Setting stock for Book {BookId} to {Quantity}", request.BookId, request.NewQuantity);
        var stock = await db.BookStocks.FirstOrDefaultAsync(bs => bs.BookId == request.BookId, ct);
        if (stock == null)
        {
            // Option 1: Create if not exists
            l.LogInformation("Stock record not found for Book {BookId}, creating new.", request.BookId);
            stock = new BookStock(Guid.NewGuid(), request.BookId, request.NewQuantity);
            await db.BookStocks.AddAsync(stock, ct);
        }
        else
        {
            // Option 2: Update existing
            try { stock.SetStock(request.NewQuantity); } catch (ArgumentOutOfRangeException ex) { return InventoryErrors.NegativeQuantity(); }
        }
        await db.SaveChangesAsync(ct);
        l.LogInformation("Successfully set stock for Book {BookId}.", request.BookId);
        return Result.Success;
    }
}