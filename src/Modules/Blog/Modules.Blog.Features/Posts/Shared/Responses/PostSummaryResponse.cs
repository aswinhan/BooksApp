using System;

namespace Modules.Blog.Features.Posts.Shared.Responses;

public record PostSummaryResponse(
    Guid Id,
    string Title,
    string Slug,
    string AuthorName,
    DateTime CreatedAtUtc,
    DateTime? PublishedAtUtc,
    bool IsPublished
// Add excerpt or comment count later if needed
);