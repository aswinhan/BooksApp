using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc; // For [FromQuery]
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Common.Application.Pagination; // Need PaginatedResponse
using Modules.Orders.Features.Shared.Responses;
using Modules.Orders.Features.Shared.Routes;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Orders.Features.GetMyOrders;

public class GetMyOrdersEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        string route = OrderRouteConsts.OrderBaseRoute + "/my-orders";

        app.MapGet(route, Handle)
           .RequireAuthorization()
           .WithName("GetMyOrders")
           // Update Produces type
           .Produces<PaginatedResponse<OrderSummaryResponse>>(StatusCodes.Status200OK)
           .ProducesProblem(StatusCodes.Status401Unauthorized)
           .ProducesProblem(StatusCodes.Status500InternalServerError)
           .WithTags("Orders");
    }

    private static async Task<IResult> Handle(
        // Add query parameters
        IGetMyOrdersHandler handler,
        ClaimsPrincipal user,
        CancellationToken cancellationToken,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        // Pass parameters to handler
        var response = await handler.HandleAsync(userId, pageNumber, pageSize, cancellationToken);

        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }
        return Results.Ok(response.Value); // Return PaginatedResponse
    }
}