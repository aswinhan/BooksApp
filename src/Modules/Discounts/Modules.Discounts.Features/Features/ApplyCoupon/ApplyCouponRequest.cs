namespace Modules.Discounts.Features.Features.ApplyCoupon;

// DTO for the apply coupon request body
public record ApplyCouponRequest(
    string CouponCode,
    decimal CurrentCartTotal // Need the cart total to validate against MinimumCartAmount
);