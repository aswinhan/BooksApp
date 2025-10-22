using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Blog.Infrastructure.Database;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;

namespace Modules.Blog.Features.Posts.AddComment;

internal interface IAddCommentHandler : IHandler
{
    Task<Result<Success>> HandleAsync(Guid postId, string authorId, string authorName, AddCommentRequest request, CancellationToken cancellationToken);
}

internal sealed class AddCommentHandler(
    BlogDbContext dbContext,
    ILogger<AddCommentHandler> logger)
    : IAddCommentHandler
{
    public async Task<Result<Success>> HandleAsync(
        Guid postId, string authorId, string authorName, AddCommentRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to add comment to Post {PostId} by User {AuthorId}", postId, authorId);

        // 1. Find the Post Aggregate Root (include comments only if needed for rules)
        // For just adding, we might not need to load existing comments.
        var post = await dbContext.Posts
            // .Include(p => p.Comments) // Include only if rules depend on existing comments
            .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);

        if (post is null)
        {
            logger.LogWarning("Add comment failed: Post {PostId} not found.", postId);
            return Error.NotFound("Blog.PostNotFound", $"Post with ID {postId} not found.");
        }

        // 2. Execute Domain Logic via Aggregate Root
        try
        {
            post.AddComment(authorId, authorName, request.Content);
        }
        catch (InvalidOperationException ex) // Catch potential domain rule violations
        {
            logger.LogWarning("Add comment failed for Post {PostId}: {Reason}", postId, ex.Message);
            // Map specific domain exceptions to Error types if needed
            return Error.Failure("Blog.AddCommentFailed", ex.Message);
        }

        // 3. Save changes (EF Core tracks the added comment via the Post aggregate)
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully added comment to Post {PostId} by User {AuthorId}", postId, authorId);
        return Result.Success;
    }
}