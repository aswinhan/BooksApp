using FluentValidation;

namespace Modules.Catalog.Features.Authors.UpdateAuthor;

// Reuse validation logic from CreateAuthorRequestValidator
public class UpdateAuthorRequestValidator : AbstractValidator<UpdateAuthorRequest>
{
    public UpdateAuthorRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Author name is required.")
            .MaximumLength(200).WithMessage("Author name cannot exceed 200 characters.");

        RuleFor(x => x.Biography)
            .MaximumLength(2000).WithMessage("Biography cannot exceed 2000 characters.");
    }
}