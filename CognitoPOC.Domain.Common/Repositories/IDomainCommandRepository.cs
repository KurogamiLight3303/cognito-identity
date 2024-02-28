using System.Linq.Expressions;
using CognitoPOC.Domain.Common.Models;

namespace CognitoPOC.Domain.Common.Repositories;

public interface IDomainCommandRepository<TDomainObject, TKey> : IDomainRepository <TDomainObject, TKey>
    where TDomainObject : PrimaryDomainObject<TKey>
{
    Task AddAsync(TDomainObject entity, CancellationToken cancellationToken = default);
    void Update(TDomainObject entity);
    bool Update(TDomainObject entity, Dictionary<Expression<Func<TDomainObject, object>>, object> changes);
    void Remove(TDomainObject entity);
}

public interface IDomainCommandRepository<TDomainObject> : IDomainCommandRepository <TDomainObject, Guid>
    where TDomainObject : PrimaryDomainObject<Guid>
{
}