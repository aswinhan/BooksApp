using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Wishlist.Features.Features.Shared.Routes;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Wishlist.Features.Features.RemoveItem;

public class RemoveItemFromWishlistEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapDelete(WishlistRouteConsts.RemoveItem, Handle) // DELETE
           .RequireAuthorization()
           .WithName("RemoveItemFromWishlist")
           .Produces(StatusCodes.Status204NoContent) // Success
           .ProducesProblem(StatusCodes.Status404NotFound)
           .ProducesProblem(StatusCodes.Status401Unauthorized)
           .WithTags("Wishlist");
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid bookId,
        IRemoveItemFromWishlistHandler handler,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

        var result = await handler.HandleAsync(userId, bookId, cancellationToken);
        return result.IsError ? result.Errors.ToProblem() : Results.NoContent();
    }
}