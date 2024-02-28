using CognitoPOC.Domain.Common.Models;
using CognitoPOC.Domain.Core.Common.Commands;
using CognitoPOC.Domain.Core.Features.UserAccounts.Manage.Commands;
using CognitoPOC.Domain.Internationalization;
using CognitoPOC.Domain.Repositories.UserAccounts;
using CognitoPOC.Domain.Services;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Manage.Handlers;

public class PasswordCommandHandler(
    IUserAccountQueryRepository repository,
    IAuthenticationPasswordService passwordService)
    : ICommandHandler<ChangePasswordCommand>
{
    public async ValueTask<OperationResultValue> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await repository.FindAsync(p 
            => p.IsActive && p.Id == request.Id, cancellationToken);
        if (user == null)
            return new(false, I18n.UserNotFound);
        return await passwordService.AdminChangePasswordAsync(user.Username!, request.Password, request.Permanent,
            cancellationToken);
    }
}