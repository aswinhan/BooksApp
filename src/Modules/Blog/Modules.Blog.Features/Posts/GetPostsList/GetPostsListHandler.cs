using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Blog.Features.Posts.Shared.Responses; // Use PostSummaryResponse
using Modules.Blog.Infrastructure.Database;
using Modules.Common.Application.Pagination;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results; // Use Result<>
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Blog.Features.Posts.GetPostsList;

internal interface IGetPostsListHandler : IHandler
{
    Task<Result<PaginatedResponse<PostSummaryResponse>>> HandleAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken);
}

internal sealed class GetPostsListHandler(
    BlogDbContext dbContext,
    ILogger<GetPostsListHandler> logger)
    : IGetPostsListHandler
{
    public async Task<Result<PaginatedResponse<PostSummaryResponse>>> HandleAsync(
    int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving posts list - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);

        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 5, 50); // Min 5, Max 50
        try
        {
            var query = dbContext.Posts
                .AsNoTracking()
                .Where(p => p.IsPublished) // Keep filter
                .OrderByDescending(p => p.PublishedAtUtc ?? p.CreatedAtUtc); // Keep sort

            int totalCount = await query.CountAsync(cancellationToken);

            var posts = await query
                .Skip((pageNumber - 1) * pageSize) // Apply pagination
                .Take(pageSize) // Apply pagination
                .Select(p => new PostSummaryResponse(
                    p.Id, p.Title, p.Slug, p.AuthorName, p.CreatedAtUtc, p.PublishedAtUtc, p.IsPublished
                ))
                .ToListAsync(cancellationToken);

            logger.LogInformation("Retrieved {Count} posts for page {PageNumber}.", posts.Count, pageNumber);

            var paginatedResponse = new PaginatedResponse<PostSummaryResponse>(
                posts, totalCount, pageNumber, pageSize
            );

            return paginatedResponse;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve posts list for page {PageNumber}.", pageNumber);
            return Error.Unexpected("Blog.GetListFailed", "Failed to retrieve posts list.");
        }
    }
}