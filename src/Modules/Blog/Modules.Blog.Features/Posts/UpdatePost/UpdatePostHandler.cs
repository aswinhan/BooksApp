using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Blog.Features.Posts.Shared.Responses; // For PostResponse
using Modules.Blog.Infrastructure.Database; // For BlogDbContext
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results; // For Result<>, Error

namespace Modules.Blog.Features.Posts.UpdatePost;

internal interface IUpdatePostHandler : IHandler
{
    // Pass userId for authorization check
    Task<Result<PostResponse>> HandleAsync(Guid postId, string userId, UpdatePostRequest request, CancellationToken cancellationToken);
}

internal sealed class UpdatePostHandler(
    BlogDbContext dbContext,
    ILogger<UpdatePostHandler> logger)
    : IUpdatePostHandler
{
    public async Task<Result<PostResponse>> HandleAsync(
        Guid postId, string userId, UpdatePostRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to update post with ID: {PostId} by User {UserId}", postId, userId);

        // 1. Find the Post Aggregate Root (include comments for response mapping)
        var post = await dbContext.Posts
            .Include(p => p.Comments) // Include comments to map back in response
            .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);

        if (post is null)
        {
            logger.LogWarning("Update post failed: Post {PostId} not found.", postId);
            return Error.NotFound("Blog.PostNotFound", $"Post with ID {postId} not found.");
        }

        // 2. Authorization Check (Example: Only author can edit)
        if (post.AuthorId != userId)
        {
            logger.LogWarning("Update post failed: User {UserId} is not the author of Post {PostId}.", userId, postId);
            return Error.Forbidden("Blog.NotAuthor", "User is not authorized to edit this post.");
        }


        // 3. Check for Slug Conflict (if slug changed)
        var newSlug = request.Slug.ToLowerInvariant().Replace(" ", "-"); // Sanitize slug
        if (post.Slug != newSlug)
        {
            var slugExists = await dbContext.Posts
                                        .AnyAsync(p => p.Id != postId && p.Slug == newSlug, cancellationToken);
            if (slugExists)
            {
                logger.LogWarning("Update post failed: New slug '{Slug}' already exists.", newSlug);
                return Error.Conflict("Blog.SlugExists", $"The slug '{newSlug}' is already in use.");
            }
        }




        // 4. Execute the Domain Logic via the Aggregate Root
        try
        {
            post.Update(request.Title, request.Content, newSlug, request.BlogCategoryId); 
        }
        catch (ArgumentException ex) // Catch validation errors from domain
        {
            logger.LogWarning("Update post failed due to validation: {Reason}", ex.Message);
            return Error.Validation("Blog.UpdateValidation", ex.Message);
        }
        catch (InvalidOperationException ex) // Catch potential domain rule violations
        {
            logger.LogWarning("Update post failed for Post {PostId}: {Reason}", postId, ex.Message);
            return Error.Failure("Blog.UpdateFailed", ex.Message);
        }

        // 5. Save changes (EF Core tracks the state change)
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully updated post with ID: {PostId}", postId);

        // 6. Map updated entity to Response DTO
        var response = new PostResponse(
             post.Id, post.Title, post.Content, post.AuthorId, post.AuthorName, post.Slug,
             post.IsPublished, post.PublishedAtUtc,
             post.BlogCategoryId,
             post.Tags.Select(t => t.Name).ToList(),
             post.Comments.Select(c => new CommentResponse( // Map existing comments
                 c.Id, c.PostId, c.AuthorId, c.AuthorName, c.Content, c.CreatedAtUtc
             )).ToList(),
             post.CreatedAtUtc, post.UpdatedAtUtc // UpdatedAtUtc will now have a value
         );

        return response; // Return updated post details
    }
}