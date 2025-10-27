using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Modules.Common.API.Abstractions;
using Modules.Orders.Domain.Abstractions;
using Modules.Orders.Domain.DTOs;
using Modules.Orders.Features.Shared.Routes;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Modules.Orders.Features.Cart.RemoveCoupon;

public class RemoveCouponEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapDelete(OrderRouteConsts.RemoveCoupon, Handle) // Use DELETE
           .RequireAuthorization().WithName("RemoveCartCoupon")
           .Produces<CartDto>(StatusCodes.Status200OK) // Return updated cart
           .ProducesProblem(StatusCodes.Status401Unauthorized)
           .WithTags("Orders.Cart");
    }
    private static async Task<IResult> Handle(ICartService cartSvc, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier); if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();
        var result = await cartSvc.RemoveCouponFromCartAsync(userId); // Call service
        // This service method currently doesn't return errors, just the updated cart
        return Results.Ok(result.Value); // Always OK
    }
}