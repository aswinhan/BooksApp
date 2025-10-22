using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Blog.Features.Posts.Shared.Responses;
using Modules.Blog.Features.Posts.Shared.Routes;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Blog.Features.Posts.GetPostBySlug;

public class GetPostBySlugEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet(BlogPostRouteConsts.GetPostBySlug, Handle)
           .AllowAnonymous() // Allow anyone to read posts
           .WithName("GetPostBySlug")
           .Produces<PostResponse>(StatusCodes.Status200OK)
           .ProducesProblem(StatusCodes.Status404NotFound)
           .WithTags("Blog.Posts");
    }

    private static async Task<IResult> Handle(
        [FromRoute] string slug, // Get slug from route
        IGetPostBySlugHandler handler, // Inject handler
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(slug, cancellationToken);

        if (response.IsError)
        {
            return response.Errors.ToProblem(); // Handles NotFound
        }

        return Results.Ok(response.Value); // Return PostResponse
    }
}