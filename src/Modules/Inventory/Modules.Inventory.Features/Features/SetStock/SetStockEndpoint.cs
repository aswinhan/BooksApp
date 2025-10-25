using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Inventory.Features.Features.Shared.Routes;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Inventory.Features.Features.SetStock;

public class SetStockEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPut(InventoryRouteConsts.SetStock, Handle) // PUT to set quantity
           .RequireAuthorization() // Add ManageStock policy later
           .WithName("SetStock")
           .Produces(StatusCodes.Status204NoContent)
           .ProducesValidationProblem()
           .ProducesProblem(StatusCodes.Status404NotFound)
           .WithTags("Inventory.Admin");
    }
    private static async Task<IResult> Handle(
        [FromBody] SetStockRequest request,
        IValidator<SetStockRequest> validator,
        ISetStockHandler handler, CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid) return Results.ValidationProblem(validationResult.ToDictionary());
        var result = await handler.HandleAsync(request, ct);
        return result.IsError ? result.Errors.ToProblem() : Results.NoContent();
    }
}