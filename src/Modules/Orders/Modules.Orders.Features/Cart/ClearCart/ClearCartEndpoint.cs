using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Modules.Common.API.Abstractions;
using Modules.Orders.Domain.Abstractions;
using Modules.Orders.Features.Shared.Routes;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Modules.Orders.Features.Cart.ClearCart;

public class ClearCartEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        // Use DELETE on the base cart route to clear it
        app.MapDelete(OrderRouteConsts.ClearCart, Handle)
           .RequireAuthorization() // Must be logged in
           .WithName("ClearCart")
           .Produces(StatusCodes.Status204NoContent) // Success, no body
           .ProducesProblem(StatusCodes.Status401Unauthorized)
           .WithTags("Orders.Cart");
    }

    private static async Task<IResult> Handle(
        ICartService cartService, // Inject service
        ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        // Call the service method directly
        await cartService.ClearCartAsync(userId);

        return Results.NoContent(); // Success
    }
}