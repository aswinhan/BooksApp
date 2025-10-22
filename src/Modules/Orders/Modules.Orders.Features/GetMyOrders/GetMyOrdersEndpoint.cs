using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Modules.Common.API.Abstractions;
using Modules.Orders.Features.Shared.Responses; // Use OrderSummaryResponse
using Modules.Orders.Features.Shared.Routes; // Use OrderRouteConsts
using System.Collections.Generic; // For List<>
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Orders.Features.GetMyOrders;

public class GetMyOrdersEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        // Define a specific route, e.g., /api/orders/my-orders
        string route = OrderRouteConsts.OrderBaseRoute + "/my-orders";

        app.MapGet(route, Handle)
           .RequireAuthorization() // Must be logged in
           .WithName("GetMyOrders")
           .Produces<List<OrderSummaryResponse>>(StatusCodes.Status200OK)
           .ProducesProblem(StatusCodes.Status401Unauthorized)
           .WithTags("Orders"); // Group with other order endpoints
    }

    private static async Task<IResult> Handle(
        IGetMyOrdersHandler handler, // Inject the handler
        ClaimsPrincipal user, // Inject ClaimsPrincipal for User ID
        CancellationToken cancellationToken)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            // Should not happen due to RequireAuthorization
            return Results.Unauthorized();
        }

        var response = await handler.HandleAsync(userId, cancellationToken);

        // Assuming handler returns List<OrderSummaryResponse> directly for now
        // Refactor later to use Result<> for queries if needed
        return Results.Ok(response);
    }
}