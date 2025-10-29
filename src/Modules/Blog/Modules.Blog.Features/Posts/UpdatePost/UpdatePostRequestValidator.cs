using FluentValidation;

namespace Modules.Blog.Features.Posts.UpdatePost;

public class UpdatePostRequestValidator : AbstractValidator<UpdatePostRequest>
{
    public UpdatePostRequestValidator()
    {
        // Re-use validation logic from CreatePostRequestValidator
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.");

        RuleFor(x => x.BlogCategoryId).NotEmpty().WithMessage("Category ID is required.");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required.")
            .MaximumLength(250).WithMessage("Slug cannot exceed 250 characters.")
            .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Slug must contain only lowercase letters, numbers, and hyphens, and cannot start or end with a hyphen.");
    }
}