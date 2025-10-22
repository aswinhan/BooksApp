using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Blog.Infrastructure.Database; // For BlogDbContext
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results; // For Result<>, Success, Error

namespace Modules.Blog.Features.Posts.UnpublishPost;

// Interface for the handler
internal interface IUnpublishPostHandler : IHandler
{
    Task<Result<Success>> HandleAsync(Guid postId, CancellationToken cancellationToken);
}

// Implementation of the handler
internal sealed class UnpublishPostHandler(
    BlogDbContext dbContext, // Use DbContext to load and save aggregate
    ILogger<UnpublishPostHandler> logger)
    : IUnpublishPostHandler
{
    public async Task<Result<Success>> HandleAsync(Guid postId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to unpublish post with ID: {PostId}", postId);

        // 1. Find the Post Aggregate Root
        var post = await dbContext.Posts.FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);

        if (post is null)
        {
            logger.LogWarning("Unpublish post failed: Post {PostId} not found.", postId);
            return Error.NotFound("Blog.PostNotFound", $"Post with ID {postId} not found.");
        }

        // 2. Execute the Domain Logic via the Aggregate Root
        try
        {
            post.Unpublish(); // Call the domain method
        }
        catch (InvalidOperationException ex) // Catch potential domain rule violations (if any)
        {
            logger.LogWarning("Unpublish post failed for Post {PostId}: {Reason}", postId, ex.Message);
            return Error.Failure("Blog.UnpublishFailed", ex.Message);
        }

        // 3. Save changes (EF Core tracks the state change)
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully unpublished post with ID: {PostId}", postId);
        return Result.Success; // Return success
    }
}