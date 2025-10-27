using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Common.Domain.Handlers;
using Modules.Common.Domain.Results;
using Modules.Discounts.Domain.Entities;
using Modules.Discounts.Domain.Errors;
using Modules.Discounts.Features.Features.Shared.Responses;
using Modules.Discounts.Infrastructure.Database;

namespace Modules.Discounts.Features.Features.Admin.UpdateCoupon;

internal interface IUpdateCouponHandler : IHandler { Task<Result<CouponResponse>> HandleAsync(Guid couponId, UpdateCouponRequest request, CancellationToken ct); }
internal sealed class UpdateCouponHandler(DiscountsDbContext db, ILogger<UpdateCouponHandler> l) : IUpdateCouponHandler
{
    public async Task<Result<CouponResponse>> HandleAsync(Guid couponId, UpdateCouponRequest req, CancellationToken ct)
    {
        l.LogInformation("Updating coupon {Id}", couponId);
        var coupon = await db.Coupons.FirstOrDefaultAsync(c => c.Id == couponId, ct);
        if (coupon is null) return Error.NotFound("Discount.UpdateNotFound", $"Coupon {couponId} not found.");
        try { coupon.Update(req.Type, req.Value, req.ExpiryDate, req.UsageLimit, req.MinimumCartAmount, req.IsActive); }
        catch (ArgumentException ex) { return Error.Validation("Discount.UpdateValidation", ex.Message); }
        await db.SaveChangesAsync(ct);
        l.LogInformation("Updated coupon {Id}", couponId);
        return new CouponResponse(coupon.Id, coupon.Code, coupon.Type, coupon.Value, coupon.ExpiryDate, coupon.UsageLimit, coupon.UsageCount, coupon.MinimumCartAmount, coupon.IsActive, coupon.CreatedAtUtc, coupon.UpdatedAtUtc);
    }
}