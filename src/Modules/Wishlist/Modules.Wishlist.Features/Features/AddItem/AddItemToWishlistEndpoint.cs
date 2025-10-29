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

namespace Modules.Wishlist.Features.Features.AddItem;

public class AddItemToWishlistEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost(WishlistRouteConsts.AddItem, Handle) // POST
           .RequireAuthorization()
           .WithName("AddItemToWishlist")
           .Produces(StatusCodes.Status204NoContent) // Success
           .ProducesProblem(StatusCodes.Status404NotFound) // Book not found
           .ProducesProblem(StatusCodes.Status409Conflict) // Already in wishlist
           .ProducesProblem(StatusCodes.Status401Unauthorized)
           .WithTags("Wishlist");
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid bookId,
        IAddItemToWishlistHandler handler,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

        var result = await handler.HandleAsync(userId, bookId, cancellationToken);
        return result.IsError ? result.Errors.ToProblem() : Results.NoContent();
    }
}