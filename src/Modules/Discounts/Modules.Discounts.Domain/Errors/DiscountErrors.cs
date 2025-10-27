using Modules.Common.Domain.Results;
namespace Modules.Discounts.Domain.Errors;

public static class DiscountErrors
{
    private const string Prefix = "Discount";
    public static Error NotFound(string code) => Error.NotFound($"{Prefix}.NotFound", $"Coupon with code '{code}' not found or is inactive.");
    public static Error AlreadyExists(string code) => Error.Conflict($"{Prefix}.AlreadyExists", $"Coupon code '{code}' already exists.");
    // Add specific validation errors if needed, matching those returned by Coupon.Validate
}