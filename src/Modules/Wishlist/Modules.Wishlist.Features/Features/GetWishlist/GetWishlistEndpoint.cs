using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Wishlist.Features.Features.Shared.Responses;
using Modules.Wishlist.Features.Features.Shared.Routes;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Wishlist.Features.Features.GetWishlist;

public class GetWishlistEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet(WishlistRouteConsts.GetWishlist, Handle)
           .RequireAuthorization() // Must be logged in
           .WithName("GetWishlist")
           .Produces<List<WishlistItemResponse>>(StatusCodes.Status200OK)
           .ProducesProblem(StatusCodes.Status401Unauthorized)
           .WithTags("Wishlist");
    }

    private static async Task<IResult> Handle(
        ClaimsPrincipal user,
        IGetWishlistHandler handler,
        CancellationToken cancellationToken)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

        var result = await handler.HandleAsync(userId, cancellationToken);

        return result.IsError ? result.Errors.ToProblem() : Results.Ok(result.Value);
    }
}