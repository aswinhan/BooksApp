using FluentValidation;

namespace Modules.Catalog.Features.Authors.CreateAuthor;

public class CreateAuthorRequestValidator : AbstractValidator<CreateAuthorRequest>
{
    public CreateAuthorRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Author name is required.")
            .MaximumLength(200).WithMessage("Author name cannot exceed 200 characters.");

        RuleFor(x => x.Biography)
            .MaximumLength(2000).WithMessage("Biography cannot exceed 2000 characters.");
    }
}