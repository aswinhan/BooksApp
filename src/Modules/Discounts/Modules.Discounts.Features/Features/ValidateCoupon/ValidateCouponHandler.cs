using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Discounts.Domain.Errors;
using Modules.Discounts.Infrastructure.Database;
using Modules.Discounts.PublicApi.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Discounts.Features.Features.ValidateCoupon;

// Handler for internal API call
internal interface IValidateCouponHandler : IHandler
{
    Task<Result<CouponValidationResponse>> HandleAsync(ValidateCouponRequest request, CancellationToken ct);
}

internal sealed class ValidateCouponHandler(
    DiscountsDbContext dbContext,
    ILogger<ValidateCouponHandler> logger) : IValidateCouponHandler
{
    public async Task<Result<CouponValidationResponse>> HandleAsync(ValidateCouponRequest request, CancellationToken ct)
    {
        var codeUpper = request.Code.ToUpperInvariant();
        logger.LogDebug("Validating coupon {Code} for cart total {Total}", codeUpper, request.CartTotal);

        var coupon = await dbContext.Coupons.FirstOrDefaultAsync(c => c.Code == codeUpper, ct);

        if (coupon is null || !coupon.IsActive)
        {
            logger.LogWarning("Validation failed: Coupon {Code} not found or inactive.", codeUpper);
            // Return specific error but map to a standard validation response DTO
            return new CouponValidationResponse(request.Code, 0, false); // Indicate invalid
            // Or: return DiscountErrors.NotFound(request.Code); // If you want the Result to hold the error
        }

        var validationResult = coupon.Validate(request.CartTotal);
        if (validationResult.IsError)
        {
            logger.LogWarning("Validation failed for Coupon {Code}: {ErrorCode}", codeUpper, validationResult.FirstError.Code);
            // Return error details but in the response DTO
            return new CouponValidationResponse(request.Code, 0, false); // Indicate invalid
                                                                         // Or: return validationResult.Errors;
        }

        // If valid, calculate discount
        decimal discountAmount = coupon.CalculateDiscount(request.CartTotal);
        logger.LogDebug("Coupon {Code} validated. Discount: {Amount}", codeUpper, discountAmount);
        return new CouponValidationResponse(coupon.Code, discountAmount, true); // Indicate valid
    }
}