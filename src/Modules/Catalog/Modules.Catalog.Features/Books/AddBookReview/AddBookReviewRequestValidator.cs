using FluentValidation;

namespace Modules.Catalog.Features.Books.AddBookReview;

public class AddBookReviewRequestValidator : AbstractValidator<AddBookReviewRequest>
{
    public AddBookReviewRequestValidator()
    {
        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Comment is required.")
            .MaximumLength(1000).WithMessage("Comment cannot exceed 1000 characters.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");
    }
}