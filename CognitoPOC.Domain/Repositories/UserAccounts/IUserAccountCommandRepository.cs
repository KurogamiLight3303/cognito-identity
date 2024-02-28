using CognitoPOC.Domain.Models.UserAccounts;
using CognitoPOC.Domain.Common.Repositories;

namespace CognitoPOC.Domain.Repositories.UserAccounts;

public interface IUserAccountCommandRepository : IDomainCommandRepository<UserAccountObject, Guid>
{
    Task<UserAccountObject?> FindByAliasAsync(string? alias, 
        CancellationToken cancellationToken);
    Task<bool> ExistByAliasAsync(string? alias, 
        CancellationToken cancellationToken);
    Task<Guid> GetUserId(string username,
        CancellationToken cancellationToken);
}