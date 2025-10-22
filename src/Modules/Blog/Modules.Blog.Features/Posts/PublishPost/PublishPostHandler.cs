using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Blog.Infrastructure.Database; // For BlogDbContext
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results; // For Result<>, Success, Error

namespace Modules.Blog.Features.Posts.PublishPost;

// Interface for the handler
internal interface IPublishPostHandler : IHandler
{
    Task<Result<Success>> HandleAsync(Guid postId, CancellationToken cancellationToken);
}

// Implementation of the handler
internal sealed class PublishPostHandler(
    BlogDbContext dbContext, // Use DbContext to load and save aggregate
    ILogger<PublishPostHandler> logger)
    : IPublishPostHandler
{
    public async Task<Result<Success>> HandleAsync(Guid postId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to publish post with ID: {PostId}", postId);

        // 1. Find the Post Aggregate Root
        var post = await dbContext.Posts.FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);

        if (post is null)
        {
            logger.LogWarning("Publish post failed: Post {PostId} not found.", postId);
            return Error.NotFound("Blog.PostNotFound", $"Post with ID {postId} not found.");
        }

        // 2. Execute the Domain Logic via the Aggregate Root
        try
        {
            post.Publish(); // Call the domain method
        }
        catch (InvalidOperationException ex) // Catch potential domain rule violations
        {
            logger.LogWarning("Publish post failed for Post {PostId}: {Reason}", postId, ex.Message);
            // Map specific domain exceptions to Error types if needed
            return Error.Failure("Blog.PublishFailed", ex.Message); // Or maybe Conflict/Validation?
        }

        // 3. Save changes (EF Core tracks the state change)
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully published post with ID: {PostId}", postId);
        return Result.Success; // Return success
    }
}