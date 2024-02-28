using CognitoPOC.Domain.Core.Features.UserAccounts.Authentication.Commands;
using FluentValidation;
using CognitoPOC.Domain.Internationalization;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Authentication.Validations;

public class RefreshSessionValidation : AbstractValidator<RefreshSessionCommand>
{
    public RefreshSessionValidation()
    {
        RuleFor(p => p.RefreshToken)
            .NotNull()
            .NotEmpty()
            .WithMessage(I18n.SessionExpired);
    }
}