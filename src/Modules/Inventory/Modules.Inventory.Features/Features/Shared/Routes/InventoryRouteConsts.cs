namespace Modules.Inventory.Features.Features.Shared.Routes;

internal static class InventoryRouteConsts
{
    internal const string BaseRoute = "/api/inventory";
    internal const string SetStock = BaseRoute; // PUT /api/inventory
    internal const string GetStockByBookId = BaseRoute + "/{bookId:guid}"; // GET /api/inventory/{bookId}
}