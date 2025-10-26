using FluentValidation;
namespace Modules.Catalog.Features.Categories.CreateCategory;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(150).Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").WithMessage("Invalid slug format.");
    }
}