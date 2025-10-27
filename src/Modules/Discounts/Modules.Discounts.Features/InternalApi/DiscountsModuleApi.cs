using Modules.Common.Domain.Results;
using Modules.Discounts.Features.Features.RecordCouponUsage; // Use handlers
using Modules.Discounts.Features.Features.ValidateCoupon;
using Modules.Discounts.PublicApi;
using Modules.Discounts.PublicApi.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Discounts.Features.InternalApi;

internal sealed class DiscountsModuleApi(
    IValidateCouponHandler validateCouponHandler,
    IRecordCouponUsageHandler recordCouponUsageHandler
    ) : IDiscountsModuleApi
{
    public Task<Result<CouponValidationResponse>> ValidateAndCalculateDiscountAsync(
        ValidateCouponRequest request, CancellationToken cancellationToken)
        => validateCouponHandler.HandleAsync(request, cancellationToken);

    public Task<Result<Success>> RecordCouponUsageAsync(
        RecordUsageRequest request, CancellationToken cancellationToken)
        => recordCouponUsageHandler.HandleAsync(request, cancellationToken);
}