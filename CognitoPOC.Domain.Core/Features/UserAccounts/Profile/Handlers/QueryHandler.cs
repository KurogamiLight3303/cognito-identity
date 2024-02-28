using CognitoPOC.Domain.Common.Models;
using CognitoPOC.Domain.Core.Common.Queries;
using CognitoPOC.Domain.Core.Features.UserAccounts.Profile.Queries;
using CognitoPOC.Domain.Internationalization;
using CognitoPOC.Domain.Models.UserAccounts.Resume;
using CognitoPOC.Domain.Repositories.UserAccounts;
using CognitoPOC.Domain.Services;

namespace CognitoPOC.Domain.Core.Features.UserAccounts.Profile.Handlers;

public class QueryHandler(IUserAccountQueryRepository accountCommandRepository, ISessionMetadataService metadataService)
    :
        IQueryHandler<GetProfileQuery, UserAccountResume>
{
    public async ValueTask<OperationResultValue<UserAccountResume>> Handle(GetProfileQuery request,
        CancellationToken cancellationToken)
    {
        UserAccountResume? account;
        var username = metadataService.GetCurrentUsername();
        if (!string.IsNullOrEmpty(username)
            && (account = await accountCommandRepository.FindAsync<UserAccountResume>(
                p => p.Username == username, cancellationToken)) != null)
            return new(true, account);

        return new(false, I18n.UnknownError);
    }
}