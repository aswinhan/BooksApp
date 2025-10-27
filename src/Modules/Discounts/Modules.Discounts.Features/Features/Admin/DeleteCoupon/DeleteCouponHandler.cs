using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Discounts.Domain.Errors;
using Modules.Discounts.Infrastructure.Database;

namespace Modules.Discounts.Features.Features.Admin.DeleteCoupon;

internal interface IDeleteCouponHandler : IHandler { Task<Result<Success>> HandleAsync(Guid couponId, CancellationToken ct); }
internal sealed class DeleteCouponHandler(DiscountsDbContext db, ILogger<DeleteCouponHandler> l) : IDeleteCouponHandler
{
    public async Task<Result<Success>> HandleAsync(Guid couponId, CancellationToken ct)
    {
        l.LogInformation("Deleting coupon {Id}", couponId);
        var coupon = await db.Coupons.FirstOrDefaultAsync(c => c.Id == couponId, ct);
        if (coupon is null) return Error.NotFound("Discount.DeleteNotFound", $"Coupon {couponId} not found.");
        // Optional: Check if coupon has been used? Prevent deletion? For now, allow deletion.
        db.Coupons.Remove(coupon); await db.SaveChangesAsync(ct);
        l.LogInformation("Deleted coupon {Id}", couponId);
        return Result.Success;
    }
}