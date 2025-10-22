using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Blog.Features.Posts.Shared.Responses; // Use PostSummaryResponse
using Modules.Blog.Infrastructure.Database;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results; // Use Result<>

namespace Modules.Blog.Features.Posts.GetPostsList;

internal interface IGetPostsListHandler : IHandler
{
    Task<Result<List<PostSummaryResponse>>> HandleAsync(CancellationToken cancellationToken);
}

internal sealed class GetPostsListHandler(
    BlogDbContext dbContext,
    ILogger<GetPostsListHandler> logger)
    : IGetPostsListHandler
{
    public async Task<Result<List<PostSummaryResponse>>> HandleAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving list of blog posts.");
        try
        {
            // Retrieve published posts, newest first
            var posts = await dbContext.Posts
                .AsNoTracking()
                .Where(p => p.IsPublished) // Example: Only show published posts in the list
                .OrderByDescending(p => p.PublishedAtUtc ?? p.CreatedAtUtc) // Order by publish date, fallback to created date
                                                                            // Add Skip().Take() for pagination later
                .Select(p => new PostSummaryResponse(
                    p.Id,
                    p.Title,
                    p.Slug,
                    p.AuthorName,
                    p.CreatedAtUtc,
                    p.PublishedAtUtc,
                    p.IsPublished
                ))
                .ToListAsync(cancellationToken);

            logger.LogInformation("Retrieved {Count} posts.", posts.Count);
            return posts; // Implicit conversion
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve posts list.");
            return Error.Unexpected("Blog.GetListFailed", "Failed to retrieve posts list.");
        }
    }
}