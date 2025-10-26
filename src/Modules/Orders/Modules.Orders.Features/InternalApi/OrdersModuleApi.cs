using Modules.Common.Domain.Results;
using Modules.Orders.Features.CheckPurchase; // Need handler interface
using Modules.Orders.PublicApi;
using Modules.Orders.PublicApi.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Orders.Features.InternalApi;

internal sealed class OrdersModuleApi(
    ICheckPurchaseHandler checkPurchaseHandler // Inject handler
    ) : IOrdersModuleApi
{
    public Task<Result<CheckPurchaseResponse>> CheckIfUserPurchasedBookAsync(
        CheckPurchaseRequest request, CancellationToken cancellationToken)
    {
        return checkPurchaseHandler.HandleAsync(request, cancellationToken);
    }
}