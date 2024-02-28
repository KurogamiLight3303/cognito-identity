using CognitoPOC.Domain.Models.UserAccounts;
using CognitoPOC.Domain.Common.Repositories;

namespace CognitoPOC.Domain.Repositories.UserAccounts;

public interface IUserAccountQueryRepository : IDomainQueryRepository<UserAccountObject, Guid>
{
    Task<UserAccountObject?> FindByAliasAsync(string? alias, 
        CancellationToken cancellationToken);
}