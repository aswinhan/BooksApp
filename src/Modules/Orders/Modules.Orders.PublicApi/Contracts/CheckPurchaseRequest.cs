using System;

namespace Modules.Orders.PublicApi.Contracts;

public record CheckPurchaseRequest(string UserId, Guid BookId);