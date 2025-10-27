using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Discounts.Domain.Entities; // Need Coupon
using Modules.Discounts.Domain.Errors; // Need DiscountErrors
using Modules.Discounts.Features.Features.Shared.Responses;
using Modules.Discounts.Infrastructure.Database; // Need DbContext

namespace Modules.Discounts.Features.Features.ApplyCoupon;

internal interface IApplyCouponHandler : IHandler
{
    Task<Result<AppliedCouponResponse>> HandleAsync(ApplyCouponRequest request, CancellationToken cancellationToken);
}

internal sealed class ApplyCouponHandler(
    DiscountsDbContext dbContext,
    ILogger<ApplyCouponHandler> logger)
    : IApplyCouponHandler
{
    public async Task<Result<AppliedCouponResponse>> HandleAsync(ApplyCouponRequest request, CancellationToken cancellationToken)
    {
        var couponCodeUpper = request.CouponCode.ToUpperInvariant();
        logger.LogInformation("Attempting to apply coupon code: {CouponCode} to cart total {CartTotal}",
            couponCodeUpper, request.CurrentCartTotal);

        // Find the coupon by code
        var coupon = await dbContext.Coupons
                             .FirstOrDefaultAsync(c => c.Code == couponCodeUpper, cancellationToken);

        // Check if coupon exists and is active (combine checks)
        if (coupon is null || !coupon.IsActive)
        {
            logger.LogWarning("Apply coupon failed: Coupon code {CouponCode} not found or inactive.", couponCodeUpper);
            return DiscountErrors.NotFound(request.CouponCode);
        }

        // Use the Coupon entity's validation logic
        var validationResult = coupon.Validate(request.CurrentCartTotal);
        if (validationResult.IsError)
        {
            logger.LogWarning("Apply coupon failed: Validation failed for code {CouponCode}. Reason: {ErrorCode}",
                couponCodeUpper, validationResult.FirstError.Code);
            return validationResult.Errors!; // Return the specific domain validation error
        }

        // Calculate discount using domain logic
        decimal discountAmount = coupon.CalculateDiscount(request.CurrentCartTotal);
        decimal finalTotal = request.CurrentCartTotal - discountAmount;

        logger.LogInformation("Successfully applied coupon {CouponCode}. Discount: {DiscountAmount}, Final Total: {FinalTotal}",
            couponCodeUpper, discountAmount, finalTotal);

        var response = new AppliedCouponResponse(
            coupon.Code,
            discountAmount,
            finalTotal
        );

        return response;
    }
}