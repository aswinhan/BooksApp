using System;
using System.Collections.Generic;

namespace Modules.Blog.Features.Posts.Shared.Responses;

public record PostResponse(
    Guid Id,
    string Title,
    string Content,
    string AuthorId,
    string AuthorName,
    string Slug,
    bool IsPublished,
    DateTime? PublishedAtUtc,
    Guid BlogCategoryId,
    List<string> Tags,
    List<CommentResponse> Comments, // Include comments
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);