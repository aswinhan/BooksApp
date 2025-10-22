using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Blog.Features.Posts.Shared.Responses;
using Modules.Blog.Features.Posts.Shared.Routes;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Blog.Features.Posts.CreatePost;

public class CreatePostEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost(BlogPostRouteConsts.CreatePost, Handle)
           .RequireAuthorization() // Require login to create posts
                                   // .RequireAuthorization("BlogWritersPolicy") // Or a specific policy later
           .WithName("CreatePost")
           .Produces<PostResponse>(StatusCodes.Status201Created)
           .ProducesValidationProblem()
           .ProducesProblem(StatusCodes.Status409Conflict) // Slug conflict
           .ProducesProblem(StatusCodes.Status401Unauthorized)
           .WithTags("Blog.Posts");
    }

    private static async Task<IResult> Handle(
        [FromBody] CreatePostRequest request,
        IValidator<CreatePostRequest> validator,
        ICreatePostHandler handler,
        ClaimsPrincipal user, // Get author info
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var authorId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        // Example: Get DisplayName, adjust claim type if needed
        var authorName = user.FindFirstValue("displayName") ?? user.FindFirstValue(ClaimTypes.Name) ?? "Unknown";

        if (string.IsNullOrEmpty(authorId))
        {
            return Results.Unauthorized();
        }

        var response = await handler.HandleAsync(authorId, authorName, request, cancellationToken);

        if (response.IsError)
        {
            return response.Errors.ToProblem(); // Handles Conflict, etc.
        }

        // Return 201 Created
        return Results.CreatedAtRoute("GetPostBySlug",
                                      new { slug = response.Value?.Slug },
                                      response.Value);
    }
}