using System;
using System.Collections.Generic;

namespace Modules.Orders.PublicApi.Contracts;

public record GetSalesCountForBooksRequest(List<Guid> BookIds);