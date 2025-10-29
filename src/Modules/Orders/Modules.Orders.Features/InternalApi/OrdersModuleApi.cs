using Modules.Common.Domain.Results;
using Modules.Orders.Features.CheckPurchase; // Need handler interface
using Modules.Orders.Features.GetSalesCountForBooks;
using Modules.Orders.PublicApi;
using Modules.Orders.PublicApi.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Orders.Features.InternalApi;

internal sealed class OrdersModuleApi(
    ICheckPurchaseHandler checkPurchaseHandler,
    IGetSalesCountForBooksHandler getSalesCountForBooksHandler
    ) : IOrdersModuleApi
{
    public Task<Result<CheckPurchaseResponse>> CheckIfUserPurchasedBookAsync(
        CheckPurchaseRequest request, CancellationToken cancellationToken)
    {
        return checkPurchaseHandler.HandleAsync(request, cancellationToken);
    }

    public Task<Result<int>> GetSalesCountForBooksAsync(
        GetSalesCountForBooksRequest request, CancellationToken cancellationToken)
        => getSalesCountForBooksHandler.HandleAsync(request, cancellationToken);
}