using Modules.Discounts.Domain.Enums;
using System;
namespace Modules.Discounts.Features.Features.Admin.UpdateCoupon;

public record UpdateCouponRequest(DiscountType Type, decimal Value, DateTime? ExpiryDate, int UsageLimit, decimal MinimumCartAmount, bool IsActive); // Code cannot be updated