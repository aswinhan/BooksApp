using Modules.Orders.Domain.Enums; // Required for OrderStatus
using System;

namespace Modules.Orders.Features.Shared.Responses;

// Simplified DTO for listing orders
public record OrderSummaryResponse(
    Guid Id,
    DateTime CreatedAtUtc,
    OrderStatus Status,
    decimal Total,
    int ItemCount // Add a count of items for summary view
);