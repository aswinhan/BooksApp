using System;
namespace Modules.Inventory.Features.Features.SetStock;

public record SetStockRequest(Guid BookId, int NewQuantity);