namespace Modules.Wishlist.Features.Features.Shared.Routes;

internal static class WishlistRouteConsts
{
    internal const string BaseRoute = "/api/wishlist";

    internal const string GetWishlist = BaseRoute;         // GET /api/wishlist
    internal const string AddItem = $"{BaseRoute}/{{bookId:guid}}"; // POST /api/wishlist/{bookId}
    internal const string RemoveItem = $"{BaseRoute}/{{bookId:guid}}"; // DELETE /api/wishlist/{bookId}
}