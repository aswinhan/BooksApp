using FluentValidation;

namespace Modules.Catalog.Features.Books.CreateBook;

public class CreateBookRequestValidator : AbstractValidator<CreateBookRequest>
{
    public CreateBookRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters.");

        RuleFor(x => x.Isbn)
            .NotEmpty().WithMessage("ISBN is required.")
            .Length(10, 13).WithMessage("ISBN must be 10 or 13 characters long."); // Basic ISBN length validation

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative.");

        RuleFor(x => x.AuthorId)
            .NotEmpty().WithMessage("AuthorId is required.");
    }
}