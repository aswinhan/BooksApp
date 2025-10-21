using FluentValidation;

namespace Modules.Users.Features.Users.UpdateUserRole;

public class UpdateUserRoleRequestValidator : AbstractValidator<UpdateUserRoleRequest>
{
    public UpdateUserRoleRequestValidator()
    {
        RuleFor(x => x.NewRole)
            .NotEmpty().WithMessage("New role name is required.")
            .MaximumLength(50).WithMessage("Role name cannot exceed 50 characters."); // Match Identity default max length
    }
}