using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Inventory.Features.Features.Shared.Routes;
using Modules.Inventory.PublicApi.Contracts; // Use DTO
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Inventory.Features.Features.GetStockLevel;

public class GetStockLevelEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet(InventoryRouteConsts.GetStockByBookId, Handle)
           // Decide: AllowAnonymous or RequireAuthorization (e.g., admin)?
           .AllowAnonymous() // Allow anyone to check stock?
           .WithName("GetStockLevel")
           .Produces<StockLevelDto>(StatusCodes.Status200OK)
           .WithTags("Inventory"); // Public tag?
    }
    private static async Task<IResult> Handle(
        [FromRoute] Guid bookId, IGetStockLevelHandler handler, CancellationToken ct)
    {
        var result = await handler.HandleAsync(bookId, ct);
        // GetStockLevelHandler returns 0 if not found, so no NotFound error needed here
        return result.IsError ? result.Errors.ToProblem() : Results.Ok(result.Value);
    }
}