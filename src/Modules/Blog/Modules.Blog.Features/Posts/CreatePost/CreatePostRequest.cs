namespace Modules.Blog.Features.Posts.CreatePost;

public record CreatePostRequest(
    string Title,
    string Content,
    string Slug,
    Guid BlogCategoryId
);