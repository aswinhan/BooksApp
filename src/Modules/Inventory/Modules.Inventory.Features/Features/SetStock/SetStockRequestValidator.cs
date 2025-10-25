using FluentValidation;
namespace Modules.Inventory.Features.Features.SetStock;

public class SetStockRequestValidator : AbstractValidator<SetStockRequest>
{
    public SetStockRequestValidator()
    {
        RuleFor(x => x.BookId).NotEmpty();
        RuleFor(x => x.NewQuantity).GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative.");
    }
}