using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Blog.Features.Posts.Shared.Routes; // For route constants
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using System; // For Guid
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Blog.Features.Posts.PublishPost;

public class PublishPostEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        // Use POST or PUT for state change, POST is often used for actions
        // Define a specific route for publishing
        app.MapPost(BlogPostRouteConsts.BaseRoute + "/{postId:guid}/publish", Handle)
           .RequireAuthorization() // Only authorized users (e.g., admins, authors)
                                   // .RequireAuthorization("CanPublishPostsPolicy") // Add specific policy later
           .WithName("PublishPost")
           .Produces(StatusCodes.Status204NoContent) // Success
           .ProducesProblem(StatusCodes.Status404NotFound) // Post not found
           .ProducesProblem(StatusCodes.Status400BadRequest) // If already published or other rule
           .ProducesProblem(StatusCodes.Status401Unauthorized)
           .ProducesProblem(StatusCodes.Status403Forbidden)
           .WithTags("Blog.Posts");
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid postId, // Get Post ID from route
        IPublishPostHandler handler, // Inject the handler
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