using CognitoPOC.Domain.Core.Features.UserAccounts.Passwords.Commands;
using FluentValidation;
using CognitoPOC.Domain.Internationalization;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Passwords.Validations;

public class ForgotPasswordValidation : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordValidation()
    {
        RuleFor(p => p.Alias)
            .NotNull()
            .NotEmpty()
            .EmailAddress()
            .WithMessage(I18n.VerificationSent);
    }
}