namespace Modules.Blog.Features.Posts.Shared.Routes;

internal static class BlogPostRouteConsts
{
    internal const string BaseRoute = "/api/blog/posts";

    internal const string CreatePost = BaseRoute;                // POST /api/blog/posts
    internal const string GetPostBySlug = $"{BaseRoute}/{{slug}}";   // GET /api/blog/posts/{slug}
    internal const string AddComment = $"{BaseRoute}/{{postId:guid}}/comments"; // POST /api/blog/posts/{postId}/comments
    // Add routes for GetPostsList, UpdatePost, DeletePost, PublishPost later
}