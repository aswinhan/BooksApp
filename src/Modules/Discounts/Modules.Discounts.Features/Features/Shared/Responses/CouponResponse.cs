using Modules.Discounts.Domain.Enums;
using System;

namespace Modules.Discounts.Features.Features.Shared.Responses;

// DTO for returning coupon details (used for Get, Create, Update)
public record CouponResponse(
    Guid Id,
    string Code,
    DiscountType Type,
    decimal Value,
    DateTime? ExpiryDate,
    int UsageLimit,
    int UsageCount,
    decimal MinimumCartAmount,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);