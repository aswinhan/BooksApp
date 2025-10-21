using FluentValidation;

namespace Modules.Users.Features.Users.RegisterUser;

public class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            // Add other password rules as needed (match Identity config)
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.");
        // .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain a non-alphanumeric character."); // If required

        RuleFor(x => x.DisplayName)
             .MaximumLength(100).WithMessage("Display name cannot exceed 100 characters.");

        // Optional: Validate Role if provided
        RuleFor(x => x.Role)
            .MaximumLength(50).WithMessage("Role name cannot exceed 50 characters.");
    }
}