using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Blog.Domain.Entities;
using Modules.Blog.Features.Posts.Shared.Responses;
using Modules.Blog.Infrastructure.Database;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using System.Linq; // For mapping

namespace Modules.Blog.Features.Posts.CreatePost;

internal interface ICreatePostHandler : IHandler
{
    Task<Result<PostResponse>> HandleAsync(string authorId, string authorName, CreatePostRequest request, CancellationToken cancellationToken);
}

internal sealed class CreatePostHandler(
    BlogDbContext dbContext,
    ILogger<CreatePostHandler> logger)
    : ICreatePostHandler
{
    public async Task<Result<PostResponse>> HandleAsync(
        string authorId, string authorName, CreatePostRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to create post: {Title} by User {AuthorId}", request.Title, authorId);

        // TODO: Add slug generation/validation logic if needed
        // e.g., var slug = GenerateSlug(request.Title);
        var slug = request.Slug.ToLowerInvariant().Replace(" ", "-"); // Simple example

        // Check if slug already exists
        var slugExists = await dbContext.Posts.AnyAsync(p => p.Slug == slug, cancellationToken);
        if (slugExists)
        {
            logger.LogWarning("Create post failed: Slug '{Slug}' already exists.", slug);
            return Error.Conflict("Blog.SlugExists", $"The slug '{slug}' is already in use.");
        }

        var post = new Post(
            Guid.NewGuid(),
            request.Title,
            request.Content,
            authorId,
            authorName, // Store denormalized name
            slug
        );

        await dbContext.Posts.AddAsync(post, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully created post {PostId} with Title: {Title}", post.Id, post.Title);

        // Map to Response DTO
        var response = new PostResponse(
             post.Id, post.Title, post.Content, post.AuthorId, post.AuthorName, post.Slug,
             post.IsPublished, post.PublishedAtUtc, [], // Empty comments list
             post.CreatedAtUtc, post.UpdatedAtUtc
         );

        return response;
    }

    // Optional: Helper for slug generation
    // private string GenerateSlug(string title) { ... }
}