using Modules.Discounts.Domain.Enums;
using System;

namespace Modules.Discounts.Features.Features.Admin.CreateCoupon;

public record CreateCouponRequest(
    string Code,
    DiscountType Type,
    decimal Value,
    DateTime? ExpiryDate,
    int UsageLimit,
    decimal MinimumCartAmount
);