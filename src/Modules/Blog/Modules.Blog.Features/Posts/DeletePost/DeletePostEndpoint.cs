using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Blog.Features.Posts.Shared.Routes;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Blog.Features.Posts.DeletePost;

public class DeletePostEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        // Define route for deleting a specific post by ID
        string route = BlogPostRouteConsts.BaseRoute + "/{postId:guid}";

        app.MapDelete(route, Handle)
           .RequireAuthorization() // Add specific admin/author policy later
           .WithName("DeletePost")
           .Produces(StatusCodes.Status204NoContent) // Success
           .ProducesProblem(StatusCodes.Status404NotFound) // Post not found
           .ProducesProblem(StatusCodes.Status401Unauthorized)
           .ProducesProblem(StatusCodes.Status403Forbidden) // Not allowed to delete
           .WithTags("Blog.Posts");
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid postId,
        IDeletePostHandler handler,
        ClaimsPrincipal user, // Needed for authorization check
        CancellationToken cancellationToken)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        // Pass userId to handler for authorization check
        var response = await handler.HandleAsync(postId, userId, cancellationToken);

        if (response.IsError)
        {
            // Handles NotFound, Forbidden, etc.
            return response.Errors.ToProblem();
        }

        return Results.NoContent(); // Success
    }
}