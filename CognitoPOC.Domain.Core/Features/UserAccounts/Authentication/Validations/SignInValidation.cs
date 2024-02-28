using CognitoPOC.Domain.Core.Features.UserAccounts.Authentication.Commands;
using FluentValidation;
using CognitoPOC.Domain.Internationalization;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Authentication.Validations;

public class SignInValidation : AbstractValidator<SignInCommand>
{
    public SignInValidation()
    {
        RuleFor(p => p.Alias)
            .NotNull()
            .NotEmpty()
            .EmailAddress()
            .WithMessage(I18n.InvalidCredentials);
        RuleFor(p => p.Password)
            .NotNull()
            .NotEmpty()
            .WithMessage(I18n.InvalidCredentials);
    }
}