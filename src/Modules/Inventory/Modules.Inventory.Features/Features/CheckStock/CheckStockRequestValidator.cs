using FluentValidation;
using Modules.Inventory.PublicApi.Contracts;

namespace Modules.Inventory.Features.Features.CheckStock;

// Validator for the common request DTO
public class StockAdjustmentRequestValidator : AbstractValidator<StockAdjustmentRequest>
{
    public StockAdjustmentRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Stock adjustment items cannot be empty.");

        RuleForEach(x => x.Items).SetValidator(new StockAdjustmentItemValidator());
    }
}

public class StockAdjustmentItemValidator : AbstractValidator<StockAdjustmentItem>
{
    public StockAdjustmentItemValidator()
    {
        RuleFor(x => x.BookId)
            .NotEmpty().WithMessage("BookId is required.");

        // Quantity must be positive for checking/decreasing/increasing
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be positive for stock adjustments.");
    }
}