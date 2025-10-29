using Modules.Common.Domain.Results; // Use Result<>
using Modules.Orders.PublicApi.Contracts; // Use DTOs
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Orders.PublicApi;

/// <summary>
/// Defines the public contract for interacting with the Orders module internally.
/// </summary>
public interface IOrdersModuleApi
{
    /// <summary>
    /// Checks if a specific user has purchased a specific book in a completed order.
    /// </summary>
    Task<Result<CheckPurchaseResponse>> CheckIfUserPurchasedBookAsync(
        CheckPurchaseRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the total quantity of completed sales for a specific list of book IDs.
    /// </summary>
    Task<Result<int>> GetSalesCountForBooksAsync(
        GetSalesCountForBooksRequest request, CancellationToken cancellationToken);
}