using FluentValidation;
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

namespace Modules.Blog.Features.Posts.AddComment;

public class AddCommentEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost(BlogPostRouteConsts.AddComment, Handle)
           .RequireAuthorization() // Must be logged in to comment
           .WithName("AddComment")
           .Produces(StatusCodes.Status204NoContent) // Success, no body needed
           .ProducesValidationProblem()
           .ProducesProblem(StatusCodes.Status404NotFound) // Post not found
           .ProducesProblem(StatusCodes.Status401Unauthorized)
           .WithTags("Blog.Posts.Comments"); // Separate Swagger tag
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid postId, // Get Post ID from route
        [FromBody] AddCommentRequest request,
        IValidator<AddCommentRequest> validator,
        IAddCommentHandler handler,
        ClaimsPrincipal user, // Get author info
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var authorId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        var authorName = user.FindFirstValue("displayName") ?? user.FindFirstValue(ClaimTypes.Name) ?? "Anonymous"; // Get author name

        if (string.IsNullOrEmpty(authorId))
        {
            return Results.Unauthorized();
        }

        var response = await handler.HandleAsync(postId, authorId, authorName, request, cancellationToken);

        if (response.IsError)
        {
            // Handles NotFound, etc.
            return response.Errors.ToProblem();
        }

        // Return success (HTTP 204 No Content)
        return Results.NoContent();
    }
}