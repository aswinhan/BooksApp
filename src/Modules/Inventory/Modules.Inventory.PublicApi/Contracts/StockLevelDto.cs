using System;
namespace Modules.Inventory.PublicApi.Contracts;

public record StockLevelDto(Guid BookId, int QuantityAvailable);