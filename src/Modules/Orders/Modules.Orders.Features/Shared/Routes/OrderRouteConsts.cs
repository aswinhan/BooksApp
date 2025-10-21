namespace Modules.Orders.Features.Shared.Routes;

internal static class OrderRouteConsts
{
    // Base route for cart
    internal const string CartBaseRoute = "/api/cart";
    // Base route for orders
    internal const string OrderBaseRoute = "/api/orders";

    // Cart endpoints
    internal const string GetCart = CartBaseRoute;                       // GET /api/cart
    internal const string AddCartItem = CartBaseRoute + "/items";        // POST /api/cart/items
    internal const string RemoveCartItem = CartBaseRoute + "/items/{bookId:guid}"; // DELETE /api/cart/items/{bookId}
    internal const string UpdateCartItemQuantity = CartBaseRoute + "/items/{bookId:guid}"; // PUT /api/cart/items/{bookId}
    internal const string ClearCart = CartBaseRoute;                     // DELETE /api/cart

    // Order endpoints
    internal const string Checkout = OrderBaseRoute + "/checkout";       // POST /api/orders/checkout
    // Add GetOrderById, GetMyOrders later
}