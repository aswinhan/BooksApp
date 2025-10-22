using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Orders.Features.Shared.Responses;
using Modules.Orders.Features.Shared.Routes;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Orders.Features.GetOrderById;

public class GetOrderByIdEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        // Define route for getting a specific order
        string route = OrderRouteConsts.OrderBaseRoute + "/{orderId:guid}";

        app.MapGet(route, Handle)
           .RequireAuthorization() // Must be logged in
                                   // Add policy later for owner/admin check if needed at endpoint level
           .WithName("GetOrderById")
           .Produces<OrderResponse>(StatusCodes.Status200OK)
           .ProducesProblem(StatusCodes.Status404NotFound) // Order not found
           .ProducesProblem(StatusCodes.Status401Unauthorized)
           .ProducesProblem(StatusCodes.Status403Forbidden) // Not user's order
           .WithTags("Orders");
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid orderId,
        IGetOrderByIdHandler handler,
        ClaimsPrincipal user, // Need user for authorization check
        CancellationToken cancellationToken)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        // Optional: Check if user is admin (e.g., user.IsInRole("Admin"))
        bool isAdmin = user.IsInRole("Admin"); // Assuming roles are seeded/assigned

        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        // Pass userId and isAdmin flag for authorization inside handler
        var response = await handler.HandleAsync(orderId, userId, isAdmin, cancellationToken);

        if (response.IsError)
        {
            // Handles NotFound, Forbidden, etc.
            return response.Errors.ToProblem();
        }

        return Results.Ok(response.Value); // Return OrderResponse
    }
}