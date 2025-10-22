using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Blog.Features.Posts.PublishPost;
using Modules.Blog.Features.Posts.Shared.Routes; // For route constants
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Blog.Features.Posts.UnpublishPost;

public class UnpublishPostEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        // Define a specific route for unpublishing
        app.MapPost(BlogPostRouteConsts.BaseRoute + "/{postId:guid}/unpublish", Handle)
           .RequireAuthorization() // Only authorized users (e.g., admins, authors)
                                   // .RequireAuthorization("CanManagePostsPolicy") // Add specific policy later
           .WithName("UnpublishPost")
           .Produces(StatusCodes.Status204NoContent) // Success
           .ProducesProblem(StatusCodes.Status404NotFound) // Post not found
           .ProducesProblem(StatusCodes.Status400BadRequest) // If already unpublished or other rule
           .ProducesProblem(StatusCodes.Status401Unauthorized)
           .ProducesProblem(StatusCodes.Status403Forbidden)
           .WithTags("Blog.Posts");
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid postId, // Get Post ID from route
        IUnpublishPostHandler handler, // Inject the handler
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(postId, cancellationToken);

        if (response.IsError)
        {
            // ToProblem handles NotFound, BadRequest (from domain rules)
            return response.Errors.ToProblem();
        }

        // Return success (HTTP 204 No Content)
        return Results.NoContent();
    }
}