namespace Modules.Catalog.Features.Books.Shared.Routes;

internal static class BookRouteConsts
{
    // Base route for book-related endpoints
    internal const string BaseRoute = "/api/catalog/books";

    // Specific endpoint paths
    internal const string CreateBook = BaseRoute;                // POST /api/catalog/books
    internal const string GetBookById = $"{BaseRoute}/{{bookId:guid}}"; // GET /api/catalog/books/{bookId}
    internal const string AddReview = $"{BaseRoute}/{{bookId:guid}}/reviews"; // POST /api/catalog/books/{bookId}/reviews
    // Add routes for GetBooksList, UpdateBook, DeleteBook later
}