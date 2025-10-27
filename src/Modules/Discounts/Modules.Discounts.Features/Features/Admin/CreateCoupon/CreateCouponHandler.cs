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

namespace Modules.Discounts.Features.Features.Admin.CreateCoupon;

internal interface ICreateCouponHandler : IHandler { Task<Result<CouponResponse>> HandleAsync(CreateCouponRequest request, CancellationToken ct); }
internal sealed class CreateCouponHandler(DiscountsDbContext db, ILogger<CreateCouponHandler> l) : ICreateCouponHandler
{
    public async Task<Result<CouponResponse>> HandleAsync(CreateCouponRequest req, CancellationToken ct)
    {
        var codeUpper = req.Code.ToUpperInvariant();
        l.LogInformation("Creating coupon: {Code}", codeUpper);
        if (await db.Coupons.AnyAsync(c => c.Code == codeUpper, ct))
        {
            l.LogWarning("Create coupon failed: Code '{Code}' exists.", codeUpper);
            return DiscountErrors.AlreadyExists(req.Code);
        }
        Coupon coupon; try
        {
            coupon = new Coupon(Guid.NewGuid(), codeUpper, req.Type, req.Value, req.ExpiryDate, req.UsageLimit, req.MinimumCartAmount);
        }
        catch (ArgumentException ex) { return Error.Validation("Discount.CreateValidation", ex.Message); }
        await db.Coupons.AddAsync(coupon, ct); await db.SaveChangesAsync(ct);
        l.LogInformation("Created coupon {Id} with code {Code}", coupon.Id, coupon.Code);
        return MapToResponse(coupon);
    }
    private static CouponResponse MapToResponse(Coupon c) => new(c.Id, c.Code, c.Type, c.Value, c.ExpiryDate, c.UsageLimit, c.UsageCount, c.MinimumCartAmount, c.IsActive, c.CreatedAtUtc, c.UpdatedAtUtc);
}