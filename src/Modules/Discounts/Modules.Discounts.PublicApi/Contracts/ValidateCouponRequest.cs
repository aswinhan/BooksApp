namespace Modules.Discounts.PublicApi.Contracts;

public record ValidateCouponRequest(string Code, decimal CartTotal);