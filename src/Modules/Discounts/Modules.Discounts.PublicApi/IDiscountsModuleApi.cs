using Modules.Common.Domain.Results;
using Modules.Discounts.PublicApi.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Discounts.PublicApi;

public interface IDiscountsModuleApi
{
    /// <summary>
    /// Validates a coupon code against a cart total and returns discount details.
    /// </summary>
    Task<Result<CouponValidationResponse>> ValidateAndCalculateDiscountAsync(
        ValidateCouponRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Records that a coupon has been used (e.g., increments usage count).
    /// Should be called transactionally during checkout.
    /// </summary>
    Task<Result<Success>> RecordCouponUsageAsync(
        RecordUsageRequest request, CancellationToken cancellationToken);
}