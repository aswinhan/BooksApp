using Modules.Orders.Domain.Enums;
using Modules.Orders.Domain.ValueObjects; // For Address
using System;
using System.Collections.Generic;

namespace Modules.Orders.Features.Shared.Responses;

// Detailed DTO for viewing a specific order
public record OrderResponse(
    Guid Id,
    string UserId,
    Address ShippingAddress,
    OrderStatus Status,
    decimal Total,
    List<OrderItemResponse> Items, // Include detailed items
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
// Add CouponCode, DiscountAmount later if needed
);