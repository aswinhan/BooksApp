using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Blog.Domain.Entities;
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
        int pageNumber, int pageSize, string? categorySlug, string? tagSlug, string? searchQuery,
        CancellationToken cancellationToken);
}

internal sealed class GetPostsListHandler(
    BlogDbContext dbContext,
    ILogger<GetPostsListHandler> logger)
    : IGetPostsListHandler
{
    public async Task<Result<PaginatedResponse<PostSummaryResponse>>> HandleAsync(
    int pageNumber, int pageSize, string? categorySlug, string? tagSlug, string? searchQuery,
    CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving posts list - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);

        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 5, 50); // Min 5, Max 50
        try
        {
            IQueryable<Post> query = dbContext.Posts
                .AsNoTracking()
                .Where(p => p.IsPublished); // Only show published

            // --- Apply Filtering ---
            if (!string.IsNullOrWhiteSpace(categorySlug))
            {
                query = query.Where(p => p.BlogCategory.Slug == categorySlug);
            }
            if (!string.IsNullOrWhiteSpace(tagSlug))
            {
                query = query.Where(p => p.Tags.Any(t => t.Slug == tagSlug)); // Filter by tag
            }
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                string searchTerm = $"%{searchQuery.Trim()}%";
                query = query.Where(p => EF.Functions.ILike(p.Title, searchTerm) ||
                                         EF.Functions.ILike(p.Content, searchTerm));
            }
            // --- End Filtering ---

            query = query.OrderByDescending(p => p.PublishedAtUtc ?? p.CreatedAtUtc); // Default sort

            // --- Get Count and Paginate ---
            int totalCount = await query.CountAsync(cancellationToken);
            var posts = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PostSummaryResponse( // Project to DTO
                    p.Id, p.Title, p.Slug, p.AuthorName,
                    p.CreatedAtUtc, p.PublishedAtUtc, p.IsPublished
                ))
                .ToListAsync(cancellationToken);

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