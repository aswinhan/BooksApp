using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Blog.Features.Posts.Shared.Responses;
using Modules.Blog.Infrastructure.Database;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;

namespace Modules.Blog.Features.Posts.GetPostBySlug;

internal interface IGetPostBySlugHandler : IHandler
{
    Task<Result<PostResponse>> HandleAsync(string slug, CancellationToken cancellationToken);
}

internal sealed class GetPostBySlugHandler(
    BlogDbContext dbContext,
    ILogger<GetPostBySlugHandler> logger)
    : IGetPostBySlugHandler
{
    public async Task<Result<PostResponse>> HandleAsync(string slug, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to retrieve post with slug: {Slug}", slug);

        var post = await dbContext.Posts
            .AsNoTracking()
            .Include(p => p.Comments) // Include comments for the post detail view
            .FirstOrDefaultAsync(p => p.Slug == slug, cancellationToken);

        if (post is null)
        {
            logger.LogWarning("Post with slug {Slug} not found.", slug);
            return Error.NotFound("Blog.PostNotFound", $"Post with slug '{slug}' not found.");
        }

        // Map Entity to Response DTO
        var response = new PostResponse(
            post.Id,
            post.Title,
            post.Content,
            post.AuthorId,
            post.AuthorName,
            post.Slug,
            post.IsPublished,
            post.PublishedAtUtc,
            post.BlogCategoryId,
                post.Tags.Select(t => t.Name).ToList(),
            post.Comments.Select(c => new CommentResponse( // Map included comments
                c.Id,
                c.PostId, // Often redundant here but complete
                c.AuthorId,
                c.AuthorName,
                c.Content,
                c.CreatedAtUtc
            )).ToList(),
            post.CreatedAtUtc,
            post.UpdatedAtUtc
        );

        logger.LogInformation("Successfully retrieved post with slug: {Slug}", slug);
        return response;
    }
}