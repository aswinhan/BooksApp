using FluentValidation;

namespace Modules.Orders.Features.Cart.UpdateItemQuantity;

public class UpdateItemQuantityRequestValidator : AbstractValidator<UpdateItemQuantityRequest>
{
    public UpdateItemQuantityRequestValidator()
    {
        // Quantity must be zero or more (zero means remove)
        RuleFor(x => x.NewQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative.");
    }
}