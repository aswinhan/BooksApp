namespace Modules.Blog.Features.Posts.UpdatePost;

// DTO for the update request body
public record UpdatePostRequest(
    string Title,
    string Content,
    string Slug // Allow updating the slug
);