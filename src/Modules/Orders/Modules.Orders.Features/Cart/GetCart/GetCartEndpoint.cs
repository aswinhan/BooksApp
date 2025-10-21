using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Modules.Common.API.Abstractions;
using Modules.Orders.Domain.Abstractions; // For ICartService
using Modules.Orders.Domain.DTOs; // For CartDto
using Modules.Orders.Features.Shared.Routes;
using System.Security.Claims; // For ClaimsPrincipal
using System.Threading.Tasks;

namespace Modules.Orders.Features.Cart.GetCart;

public class GetCartEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet(OrderRouteConsts.GetCart, Handle)
           .RequireAuthorization() // Must be logged in to view cart
           .WithName("GetCart")
           .Produces<CartDto>(StatusCodes.Status200OK)
           .ProducesProblem(StatusCodes.Status401Unauthorized)
           .WithTags("Orders.Cart");
    }

    private static async Task<IResult> Handle(
        ICartService cartService, // Inject the service directly
        ClaimsPrincipal user) // Inject ClaimsPrincipal to get User ID
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        var cart = await cartService.GetCartAsync(userId);
        return Results.Ok(cart);
    }
}