using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CognitoPOC.Domain.Common.Models;
using CognitoPOC.Domain.Models.UserAccounts;
using CognitoPOC.Domain.Repositories.UserAccounts;
using CognitoPOC.Infrastructure.Persistence.Contexts;

namespace CognitoPOC.Infrastructure.Persistence.Repositories.UserAccounts;

public class UserAccountsRepository(
    DomainContext context,
    IMapper? mapper = null,
    IQueryFilterTranslator<UserAccountObject, Guid>? filterTranslator = null)
    :
        BaseEfEntityRepository<UserAccountObject, Guid>(context, mapper, filterTranslator),
        IUserAccountCommandRepository
{
    public Task<UserAccountObject?> FindByAliasAsync(string? alias, CancellationToken cancellationToken)
    {
        return GetQueryable.FirstOrDefaultAsync(p => p.Username == alias, cancellationToken);
    }

    public Task<bool> ExistByAliasAsync(string? alias, CancellationToken cancellationToken)
    {
        return DataSet.AsNoTracking().AnyAsync(p => p.Username == alias, cancellationToken);
    }

    public Task<Guid> GetUserId(string username, CancellationToken cancellationToken)
    {
        return DataSet.AsNoTracking()
            .Where(p => p.Username == username)
            .Select(p => p.Id)
            .FirstOrDefaultAsync(cancellationToken:cancellationToken);
    }
}