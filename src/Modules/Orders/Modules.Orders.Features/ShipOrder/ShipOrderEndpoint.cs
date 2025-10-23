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

namespace Modules.Orders.Features.ShipOrder;

public class ShipOrderEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        // Define route for the ship action
        string route = OrderRouteConsts.OrderBaseRoute + "/{orderId:guid}/ship";

        app.MapPost(route, Handle) // Use POST for actions/state changes
           .RequireAuthorization(OrderPolicyConsts.ManageOrdersPolicy)
           .WithName("ShipOrder")
           .Produces(StatusCodes.Status204NoContent) // Success
           .ProducesProblem(StatusCodes.Status404NotFound) // Order not found
           .ProducesProblem(StatusCodes.Status400BadRequest) // Invalid status transition
           .ProducesProblem(StatusCodes.Status401Unauthorized)
           .ProducesProblem(StatusCodes.Status403Forbidden)
           .WithTags("Orders.Admin"); // Separate tag for admin actions
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid orderId,
        IShipOrderHandler handler, // Inject handler
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(orderId, cancellationToken);

        if (response.IsError)
        {
            // Handles NotFound, BadRequest (from domain rule)
            return response.Errors.ToProblem();
        }

        return Results.NoContent(); // Success
    }
}