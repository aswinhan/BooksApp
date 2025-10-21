using FluentValidation;

namespace Modules.Users.Features.Users.RefreshToken;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        // Basic validation: ensure tokens are provided
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Expired JWT token is required.");

        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}