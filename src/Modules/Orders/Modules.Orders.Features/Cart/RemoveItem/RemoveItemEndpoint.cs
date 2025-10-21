using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc; // For FromRoute
using Modules.Common.API.Abstractions;
using Modules.Orders.Domain.Abstractions; // For ICartService
using Modules.Orders.Features.Shared.Routes;
using System; // For Guid
using System.Security.Claims;
using System.Threading.Tasks;

namespace Modules.Orders.Features.Cart.RemoveItem;

public class RemoveItemEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapDelete(OrderRouteConsts.RemoveCartItem, Handle) // Use DELETE
           .RequireAuthorization() // Must be logged in
           .WithName("RemoveCartItem")
           .Produces(StatusCodes.Status204NoContent) // Success, no body
           .ProducesProblem(StatusCodes.Status404NotFound) // Item not in cart? (Handled by service)
           .ProducesProblem(StatusCodes.Status401Unauthorized)
           .WithTags("Orders.Cart");
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid bookId, // Get Book ID from route
        ICartService cartService, // Inject service
        ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        // Call the service method directly
        await cartService.RemoveItemFromCartAsync(userId, bookId);

        // Always return NoContent for DELETE success
        return Results.NoContent();
    }
}