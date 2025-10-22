using FluentValidation;

namespace Modules.Blog.Features.Posts.AddComment;

public class AddCommentRequestValidator : AbstractValidator<AddCommentRequest>
{
    public AddCommentRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Comment content cannot be empty.")
            .MaximumLength(2000).WithMessage("Comment cannot exceed 2000 characters.");
    }
}