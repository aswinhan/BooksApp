namespace Modules.Discounts.Features.Features.Shared.Routes;

internal static class DiscountRouteConsts
{
    // Public endpoint
    internal const string ApplyCoupon = "/api/discounts/apply"; // POST

    // Admin endpoints
    internal const string AdminBaseRoute = "/api/admin/discounts";
    internal const string CreateCoupon = AdminBaseRoute;       // POST
    internal const string GetCoupons = AdminBaseRoute;         // GET
    internal const string UpdateCoupon = AdminBaseRoute + "/{couponId:guid}"; // PUT
    internal const string DeleteCoupon = AdminBaseRoute + "/{couponId:guid}"; // DELETE
}