using CognitoPOC.Domain.Common.Models;
using CognitoPOC.Domain.Core.Common.Commands;
using CognitoPOC.Domain.Core.Common.Queries;
using CognitoPOC.Domain.Core.Features.UserAccounts.Manage.Commands;
using CognitoPOC.Domain.Core.Features.UserAccounts.Manage.Queries;
using CognitoPOC.Domain.Internationalization;
using CognitoPOC.Domain.Models.UserAccounts;
using CognitoPOC.Domain.Repositories.UserAccounts;
using CognitoPOC.Domain.Services;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Manage.Handlers;

public class RolesHandler(IUserAccountQueryRepository repository, IAuthenticationRolesService rolesService)
    :
        ICollectionQueryHandler<GetUserRolesQuery, string>,
        ICollectionQueryHandler<GetRolesQuery, string>,
        ICommandHandler<UpdateUserRolesCommand>
{
    public async ValueTask<QueryResult<string>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
    {
        var user = await repository.FindAsync(p 
            => p.IsActive && p.Id == request.Id, cancellationToken);
        if (user == null)
            return new(I18n.UserNotFound);
        return new(await rolesService.GetRoles(user, cancellationToken));
    }

    public async ValueTask<OperationResultValue> Handle(UpdateUserRolesCommand request, CancellationToken cancellationToken)
    {
        var validRoles = Roles.GetRoles();
        if(request.Roles == null)
            return new(true, I18n.RolesUpdated);
        var role = request.Roles.FirstOrDefault(p => !validRoles.Contains(p));
        if (role != null)
            return new(false, string.Format(I18n.InvalidRole, role));
        var user = await repository.FindAsync(p 
            => p.IsActive && p.Id == request.Id, cancellationToken);
        if (user == null)
            return new(false, I18n.UserNotFound);
        return await rolesService.UpdateRoles(user, request.Roles!, cancellationToken);
    }

    public ValueTask<QueryResult<string>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(new QueryResult<string>(Roles.GetRoles()));
    }
}