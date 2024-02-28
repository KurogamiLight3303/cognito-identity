using System.Linq.Expressions;
using CognitoPOC.Domain.Common.Models;

namespace CognitoPOC.Domain.Common.Repositories;

public interface IDomainQueryRepository<TDomainObject> : IDomainQueryRepository<TDomainObject, Guid>
    where TDomainObject : DomainObject<Guid>
{
    
}

public interface IDomainQueryRepository<TDomainObject, TKey> : IDomainRepository<TDomainObject, TKey> 
    where TDomainObject : DomainObject<TKey>
{
    Task<PagedResultValue<TProjectedValue>> SearchAsync<TProjectedValue>(PagedFilteredRequestValue parameters,
        Expression<Func<TDomainObject, bool>> condition, CancellationToken cancellationToken = default);
    Task<PagedResultValue<TProjectedValue>> SearchAsync<TProjectedValue>(PagedFilteredRequestValue parameters,
        CancellationToken cancellationToken = default) where TProjectedValue : DomainResume<TDomainObject>;
    Task<PagedResultValue<TDomainObject>> SearchAsync(PagedFilteredRequestValue parameters,
        CancellationToken cancellationToken = default);
    Task<List<TProjectedValue>> AllAsync<TProjectedValue>(
        Expression<Func<TDomainObject, bool>> condition,
        CancellationToken cancellationToken)
        where TProjectedValue : DomainResume<TDomainObject>;
    Task<List<TProjectedValue>> AllAsync<TProjectedValue>(
        CancellationToken cancellationToken)
        where TProjectedValue : DomainResume<TDomainObject>;
    IQueryable<TProjectedValue> QueryAll<TProjectedValue>(
        Expression<Func<TDomainObject, bool>> condition)
        where TProjectedValue : DomainResume<TDomainObject>;
    IQueryable<TProjectedValue> QueryAll<TProjectedValue>(
        Expression<Func<TDomainObject, bool>> condition, Expression<Func<TDomainObject, object>> orderBy)
        where TProjectedValue : DomainResume<TDomainObject>;
    IQueryable<TProjectedValue> QueryAll<TProjectedValue>()
        where TProjectedValue : DomainResume<TDomainObject>;
}