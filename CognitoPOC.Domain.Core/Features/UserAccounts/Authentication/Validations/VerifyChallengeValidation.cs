using CognitoPOC.Domain.Core.Features.UserAccounts.Authentication.Commands;
using FluentValidation;
using CognitoPOC.Domain.Internationalization;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Authentication.Validations;

public class VerifyChallengeValidation : AbstractValidator<VerifyChallengeCommand>
{
    public VerifyChallengeValidation()
    {
        RuleFor(p => p.Session)
            .NotNull()
            .NotEmpty()
            .WithMessage(I18n.SessionExpired);
        RuleFor(p => p.Username)
            .NotNull()
            .NotEmpty()
            .WithMessage(I18n.SessionExpired);
    }
}