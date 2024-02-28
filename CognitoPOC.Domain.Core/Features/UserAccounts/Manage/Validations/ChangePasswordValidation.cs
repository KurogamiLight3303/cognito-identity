using CognitoPOC.Domain.Core.Features.UserAccounts.Manage.Commands;
using FluentValidation;
using CognitoPOC.Domain.Internationalization;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Manage.Validations;

public class ChangePasswordValidation : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidation()
    {
        RuleFor(p => p.Password)
            .MinimumLength(8)
            .Must(p => p.Any(char.IsNumber))
            .Must(p => p.Any(char.IsLetter))
            .Must(p => p.Any(char.IsLower))
            .Must(p => p.Any(char.IsUpper))
            .WithMessage(I18n.WeakPassword);
    }
}