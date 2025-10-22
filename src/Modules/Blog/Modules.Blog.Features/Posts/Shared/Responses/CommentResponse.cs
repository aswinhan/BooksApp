using System;

namespace Modules.Blog.Features.Posts.Shared.Responses;

public record CommentResponse(
    Guid Id,
    Guid PostId,
    string AuthorId,
    string AuthorName,
    string Content,
    DateTime CreatedAtUtc
);