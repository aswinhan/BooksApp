namespace Modules.Catalog.Features.Categories.Shared.Routes;

internal static class CategoryRouteConsts
{
    internal const string BaseRoute = "/api/catalog/categories";
    internal const string CreateCategory = BaseRoute;         // POST
    internal const string GetCategoriesList = BaseRoute;        // GET
    internal const string UpdateCategory = $"{BaseRoute}/{{categoryId:guid}}"; // PUT
    internal const string DeleteCategory = $"{BaseRoute}/{{categoryId:guid}}"; // DELETE
}