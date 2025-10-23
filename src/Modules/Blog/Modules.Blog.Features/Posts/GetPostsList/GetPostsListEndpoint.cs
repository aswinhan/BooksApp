using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc; // For [FromQuery]
using Modules.Blog.Features.Posts.Shared.Responses;
using Modules.Blog.Features.Posts.Shared.Routes;
using Modules.Common.API.Abstractions;
using Modules.Common.API.Extensions;
using Modules.Common.Application.Pagination; // Need PaginatedResponse
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Blog.Features.Posts.GetPostsList;

public class GetPostsListEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet(BlogPostRouteConsts.BaseRoute, Handle)
           .AllowAnonymous()
           .WithName("GetPostsList")
           // Update Produces type
           .Produces<PaginatedResponse<PostSummaryResponse>>(StatusCodes.Status200OK)
           .ProducesProblem(StatusCodes.Status500InternalServerError)
           .WithTags("Blog.Posts");
    }

    private static async Task<IResult> Handle(
        // Add query parameters
        IGetPostsListHandler handler,
        CancellationToken cancellationToken,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
        
    {
        // Pass parameters to handler
        var response = await handler.HandleAsync(pageNumber, pageSize, cancellationToken);

        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }
        return Results.Ok(response.Value); // Return PaginatedResponse
    }
}