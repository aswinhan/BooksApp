using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Blog.Domain.Policies;
using Modules.Blog.Features.Posts.Shared.Responses; // For PostResponse
using Modules.Blog.Features.Posts.Shared.Routes;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using System;
using System.Security.Claims; // Needed if checking author
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Blog.Features.Posts.UpdatePost;

public class UpdatePostEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        // Define a route parameter for the post ID to update
        string route = BlogPostRouteConsts.BaseRoute + "/{postId:guid}";

        // Use PUT for updating the entire resource representation (or PATCH for partial)
        app.MapPut(route, Handle)
           .RequireAuthorization(BlogPostPolicyConsts.ManageAllPostsPolicy)
           .WithName("UpdatePost")
           .Produces<PostResponse>(StatusCodes.Status200OK) // Return updated post
           .ProducesValidationProblem()
           .ProducesProblem(StatusCodes.Status404NotFound) // Post not found
           .ProducesProblem(StatusCodes.Status409Conflict) // Slug conflict
           .ProducesProblem(StatusCodes.Status401Unauthorized)
           .ProducesProblem(StatusCodes.Status403Forbidden) // Not allowed to edit
           .WithTags("Blog.Posts");
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid postId, // Get ID from route
        [FromBody] UpdatePostRequest request,
        IValidator<UpdatePostRequest> validator,
        IUpdatePostHandler handler,
        ClaimsPrincipal user, // Needed for authorization checks
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier); // Get current user ID
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }


        // Pass user ID to handler for authorization checks if needed
        var response = await handler.HandleAsync(postId, userId, request, cancellationToken);

        if (response.IsError)
        {
            // ToProblem handles NotFound, Conflict, Forbidden, etc.
            return response.Errors.ToProblem();
        }

        // Return success (HTTP 200 OK with updated PostResponse)
        return Results.Ok(response.Value);
    }
}