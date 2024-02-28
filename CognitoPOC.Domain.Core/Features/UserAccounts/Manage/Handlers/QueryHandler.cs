using CognitoPOC.Domain.Common.Models;
using CognitoPOC.Domain.Core.Common.Queries;
using CognitoPOC.Domain.Core.Features.UserAccounts.Manage.Queries;
using CognitoPOC.Domain.Internationalization;
using CognitoPOC.Domain.Models.UserAccounts.Resume;
using CognitoPOC.Domain.Repositories.UserAccounts;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Manage.Handlers;

public class QueryHandler(IUserAccountQueryRepository repository) :
    IPagedQueryHandler<SearchUserQuery, UserAccountBaseResume>,
    IQueryHandler<GetUserQuery, UserAccountBaseResume>
{
    public async ValueTask<PagedResultValue<UserAccountBaseResume>> Handle(SearchUserQuery request, CancellationToken cancellationToken)
    {
        return await repository.SearchAsync<UserAccountBaseResume>(request, cancellationToken);
    }

    public async ValueTask<OperationResultValue<UserAccountBaseResume>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await repository.FindAsync<UserAccountBaseResume>(p 
            => p.Id == request.Id, cancellationToken);
        return user != null 
            ? new(true, user)
            : new(false, I18n.UserNotFound);
    }
}