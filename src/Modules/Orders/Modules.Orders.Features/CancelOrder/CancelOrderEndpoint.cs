using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Orders.Domain.Policies;
using Modules.Orders.Features.Shared.Routes;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Orders.Features.CancelOrder;

public class CancelOrderEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        string route = OrderRouteConsts.OrderBaseRoute + "/{orderId:guid}/cancel"; // Specific route

        app.MapPost(route, Handle) // Use POST for action
           .RequireAuthorization(OrderPolicyConsts.ManageOrdersPolicy)
           .WithName("CancelOrder")
           .Produces(StatusCodes.Status204NoContent) // Success
           .ProducesProblem(StatusCodes.Status404NotFound)
           .ProducesProblem(StatusCodes.Status400BadRequest) // Invalid transition
           .ProducesProblem(StatusCodes.Status401Unauthorized)
           .ProducesProblem(StatusCodes.Status403Forbidden)
           .WithTags("Orders"); // Or Orders.Admin depending on who can cancel
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid orderId,
        ICancelOrderHandler handler, // Inject handler
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(orderId, cancellationToken);

        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.NoContent(); // Success
    }
}