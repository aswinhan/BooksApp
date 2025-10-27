namespace Modules.Discounts.PublicApi.Contracts;

public record CouponValidationResponse(string Code, decimal DiscountAmount, bool IsValid); // Include validity flag explicitly