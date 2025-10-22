namespace Modules.Catalog.Features.Authors.Shared.Routes;

internal static class AuthorRouteConsts
{
    internal const string BaseRoute = "/api/catalog/authors";

    internal const string CreateAuthor = BaseRoute;         // POST /api/catalog/authors
    internal const string GetAuthorsList = BaseRoute;        // GET /api/catalog/authors
    internal const string UpdateAuthor = $"{BaseRoute}/{{authorId:guid}}"; // PUT /api/catalog/authors/{authorId}
    // Add GetAuthorById, DeleteAuthor later if needed
}