using CognitoPOC.Domain.Core.Features.UserAccounts.Manage.Commands;
using FluentValidation;
using CognitoPOC.Domain.Internationalization;
using CognitoPOC.Domain.Repositories.UserAccounts;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Manage.Validations;

public class CreateUserValidation : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidation(IUserAccountQueryRepository repository)
    {
        RuleFor(p => p.Email)
            .MustAsync(async (v, cc) 
                => !await repository.AnyAsync(p => p.Username == v, cc))
            .WithMessage(I18n.UserAlreadyExist);
        RuleFor(p => p.Password)
            .MinimumLength(8)
            .Must(p => p.Any(char.IsNumber))
            .Must(p => p.Any(char.IsLetter))
            .Must(p => p.Any(char.IsLower))
            .Must(p => p.Any(char.IsUpper))
            .WithMessage(I18n.WeakPassword);
    }
}