using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Blog.Infrastructure.Database;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;

namespace Modules.Blog.Features.Posts.DeletePost;

internal interface IDeletePostHandler : IHandler
{
    Task<Result<Success>> HandleAsync(Guid postId, string userId, CancellationToken cancellationToken);
}

internal sealed class DeletePostHandler(
    BlogDbContext dbContext,
    ILogger<DeletePostHandler> logger)
    : IDeletePostHandler
{
    public async Task<Result<Success>> HandleAsync(Guid postId, string userId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to delete post with ID: {PostId} by User {UserId}", postId, userId);

        // 1. Find the Post (include comments ONLY if cascade delete isn't configured)
        var post = await dbContext.Posts
            // .Include(p => p.Comments)
            .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);

        if (post is null)
        {
            logger.LogWarning("Delete post failed: Post {PostId} not found.", postId);
            return Error.NotFound("Blog.PostNotFound", $"Post with ID {postId} not found.");
        }

        // 2. Authorization Check (Example: Only author or Admin can delete)
        // You'll need a way to check if userId belongs to an Admin role here
        bool isAdmin = false; // Replace with actual role check logic later
        if (post.AuthorId != userId && !isAdmin)
        {
            logger.LogWarning("Delete post failed: User {UserId} is not authorized to delete Post {PostId}.", userId, postId);
            return Error.Forbidden("Blog.NotAuthorized", "User is not authorized to delete this post.");
        }

        // 3. Remove the post
        dbContext.Posts.Remove(post);

        // Note: EF Core with cascade delete configured (default for required relationships)
        // should automatically remove associated Comments. Verify this behavior.
        // If not cascading: dbContext.Comments.RemoveRange(post.Comments);

        // 4. Save changes
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully deleted post with ID: {PostId}", postId);

        return Result.Success;
    }
}