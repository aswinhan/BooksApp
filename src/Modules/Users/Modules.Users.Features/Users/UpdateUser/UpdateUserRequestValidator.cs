using FluentValidation;

namespace Modules.Users.Features.Users.UpdateUser;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.");

        RuleFor(x => x.DisplayName)
            .MaximumLength(100).WithMessage("Display name cannot exceed 100 characters.");

        // Optional: Validate Role if provided
        RuleFor(x => x.Role)
           .MaximumLength(50).WithMessage("Role name cannot exceed 50 characters.")
           .When(x => !string.IsNullOrWhiteSpace(x.Role)); // Only validate if not empty
    }
}