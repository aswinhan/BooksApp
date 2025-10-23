namespace Modules.Blog.Domain.Policies;

public static class BlogPostPolicyConsts
{
    // Policy for users who can create, edit, delete, publish/unpublish ANY post (Admin/Editor)
    public const string ManageAllPostsPolicy = "blog:manage:all";

    // Policy for users who can edit/delete THEIR OWN posts (Author)
    // (We'll implement this logic with RequireAssertion later)
    public const string ManageOwnPostsPolicy = "blog:manage:own";

    // Policy for adding comments (usually any authenticated user)
    public const string AddCommentsPolicy = "blog:comments:add";
}