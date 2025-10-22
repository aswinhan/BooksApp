using System;

namespace Modules.Orders.Features.Shared.Responses;

public record OrderItemResponse(
    Guid Id,
    Guid BookId,
    string BookTitle,
    decimal Price,
    int Quantity
);