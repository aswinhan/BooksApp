using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Modules.Blog.Features.Posts.Shared.Responses; // Use PostSummaryResponse (to be created)
using Modules.Blog.Features.Posts.Shared.Routes;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Blog.Features.Posts.GetPostsList;

public class GetPostsListEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        // GET on the base route for posts
        app.MapGet(BlogPostRouteConsts.BaseRoute, Handle)
           .AllowAnonymous() // Allow anyone to list posts
           .WithName("GetPostsList")
           .Produces<List<PostSummaryResponse>>(StatusCodes.Status200OK)
           .WithTags("Blog.Posts");
    }

    private static async Task<IResult> Handle(
        IGetPostsListHandler handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(cancellationToken);
        // Assuming handler returns Result<List<PostSummaryResponse>>
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }
        return Results.Ok(response.Value);
    }
}