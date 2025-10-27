using FluentValidation;

namespace Modules.Users.Features.Users.UpdateMyProfile;

public class UpdateMyProfileRequestValidator : AbstractValidator<UpdateMyProfileRequest>
{
    public UpdateMyProfileRequestValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display Name is required.")
            .MaximumLength(100).WithMessage("Display Name cannot exceed 100 characters.");

        // Optional validation for address fields if needed (e.g., max lengths)
        RuleFor(x => x.Street).MaximumLength(200);
        RuleFor(x => x.City).MaximumLength(100);
        RuleFor(x => x.State).MaximumLength(100);
        RuleFor(x => x.ZipCode).MaximumLength(20);
    }
}