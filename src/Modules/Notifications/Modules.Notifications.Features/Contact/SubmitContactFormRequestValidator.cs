using FluentValidation;
namespace Modules.Notifications.Features.Contact;

public class SubmitContactFormRequestValidator : AbstractValidator<SubmitContactFormRequest>
{
    public SubmitContactFormRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(150);
        RuleFor(x => x.Message).NotEmpty().MaximumLength(2000);
    }
}