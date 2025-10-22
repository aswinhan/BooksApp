namespace Modules.Blog.Features.Posts.CreatePost;

public record CreatePostRequest(
    string Title,
    string Content,
    string Slug // Allow user to suggest slug, or generate in handler
);