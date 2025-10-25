using System;
using System.Collections.Generic;

namespace Modules.Inventory.PublicApi.Contracts;

// Represents a request to adjust stock for one or more books
public record StockAdjustmentRequest(List<StockAdjustmentItem> Items);

public record StockAdjustmentItem(Guid BookId, int Quantity); // Quantity is positive for increase, negative for decrease