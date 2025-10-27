using FluentValidation;
namespace Modules.Orders.Features.Cart.ApplyCoupon;

public class ApplyCouponRequestValidator : AbstractValidator<ApplyCouponRequestDto>
{
    public ApplyCouponRequestValidator() { RuleFor(x => x.CouponCode).NotEmpty().MaximumLength(50); }
}