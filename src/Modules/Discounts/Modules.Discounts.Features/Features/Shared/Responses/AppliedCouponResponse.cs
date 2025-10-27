namespace Modules.Discounts.Features.Features.Shared.Responses;

// DTO for the result of applying a coupon
public record AppliedCouponResponse(
    string Code,
    decimal DiscountAmount,
    decimal FinalCartTotal // The total after discount
);