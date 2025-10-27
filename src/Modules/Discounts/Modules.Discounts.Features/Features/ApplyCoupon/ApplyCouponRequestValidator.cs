using FluentValidation;

namespace Modules.Discounts.Features.Features.ApplyCoupon;

public class ApplyCouponRequestValidator : AbstractValidator<ApplyCouponRequest>
{
    public ApplyCouponRequestValidator()
    {
        RuleFor(x => x.CouponCode)
            .NotEmpty().WithMessage("Coupon code is required.")
            .MaximumLength(50).WithMessage("Coupon code is too long.");

        RuleFor(x => x.CurrentCartTotal)
            .GreaterThanOrEqualTo(0).WithMessage("Cart total cannot be negative.");
    }
}