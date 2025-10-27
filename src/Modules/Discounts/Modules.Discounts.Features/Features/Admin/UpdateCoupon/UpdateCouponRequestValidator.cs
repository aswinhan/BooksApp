using FluentValidation;
using Modules.Discounts.Domain.Enums;
using System;
namespace Modules.Discounts.Features.Features.Admin.UpdateCoupon;

public class UpdateCouponRequestValidator : AbstractValidator<UpdateCouponRequest>
{
    public UpdateCouponRequestValidator()
    { /* Same rules as CreateCouponRequestValidator */
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Value).GreaterThan(0);
        RuleFor(x => x.UsageLimit).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MinimumCartAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ExpiryDate).GreaterThanOrEqualTo(DateTime.UtcNow).When(x => x.ExpiryDate.HasValue);
        RuleFor(x => x.Value).LessThanOrEqualTo(100).When(x => x.Type == DiscountType.Percentage);
    }
}