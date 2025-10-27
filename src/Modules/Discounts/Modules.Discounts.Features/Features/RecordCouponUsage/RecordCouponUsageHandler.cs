using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Discounts.Domain.Errors;
using Modules.Discounts.Infrastructure.Database;
using Modules.Discounts.PublicApi.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Discounts.Features.Features.RecordCouponUsage;

// Handler for internal API call
internal interface IRecordCouponUsageHandler : IHandler
{
    Task<Result<Success>> HandleAsync(RecordUsageRequest request, CancellationToken ct);
}

internal sealed class RecordCouponUsageHandler(
    DiscountsDbContext dbContext,
    ILogger<RecordCouponUsageHandler> logger) : IRecordCouponUsageHandler
{
    public async Task<Result<Success>> HandleAsync(RecordUsageRequest request, CancellationToken ct)
    {
        var codeUpper = request.Code.ToUpperInvariant();
        logger.LogInformation("Recording usage for coupon {Code}", codeUpper);

        // Find coupon - must exist if validated previously in checkout
        var coupon = await dbContext.Coupons.FirstOrDefaultAsync(c => c.Code == codeUpper, ct);

        if (coupon is null) // Should ideally not happen if validated before
        {
            logger.LogError("Record usage failed: Coupon {Code} not found.", codeUpper);
            // Return error, might indicate inconsistency
            return Error.NotFound("Discount.RecordUsageNotFound", $"Coupon {codeUpper} not found during usage recording.");
        }

        // Call domain logic to record usage
        var recordResult = coupon.RecordUsage();
        if (recordResult.IsError)
        {
            // This might happen in rare race conditions if limit was just reached
            logger.LogWarning("Record usage failed for Coupon {Code}: {ErrorCode}", codeUpper, recordResult.FirstError.Code);
            return recordResult.Errors!;
        }

        // Save changes to update UsageCount
        // Note: If called within Checkout transaction, this SaveChanges might be redundant
        // but call it anyway for atomicity within this handler's scope.
        // The transaction manager should handle nesting or enlistment.
        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation("Successfully recorded usage for coupon {Code}", codeUpper);
        return Result.Success;
    }
}