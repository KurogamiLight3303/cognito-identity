using CognitoPOC.Domain.Core.Features.UserAccounts.MFA.Commands;
using FluentValidation;
using CognitoPOC.Domain.Internationalization;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.MFA.Validations;

public class VerifySoftwareTokenValidation : AbstractValidator<VerifySoftwareTokenCommand>
{
    public VerifySoftwareTokenValidation()
    {
        RuleFor(p => p.Code)
            .NotNull()
            .NotEmpty()
            .WithMessage(I18n.InvalidVerificationCode);
    }
}