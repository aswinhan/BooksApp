using FluentValidation;

namespace Modules.Orders.Features.Cart.AddItem;

public class AddItemRequestValidator : AbstractValidator<AddItemRequest>
{
    public AddItemRequestValidator()
    {
        RuleFor(x => x.BookId)
            .NotEmpty().WithMessage("BookId is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be positive.");
    }
}