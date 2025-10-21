using System;

namespace Modules.Orders.Features.Cart.AddItem;

// DTO matching CartItemDto fields needed for adding
public record AddItemRequest(Guid BookId, int Quantity);