using System.Linq.Expressions;
using CognitoPOC.Domain.Common.Models;

namespace CognitoPOC.Domain.Common.Repositories;

public interface IDomainRepository<TDomainObject, TPrimaryKey> : IDomainRepository where TDomainObject : DomainObject<TPrimaryKey>
{
    Task<TDomainObject?> FindAsync(Expression<Func<TDomainObject, bool>> condition, 
        CancellationToken cancellationToken = default);

    Task<TProjectedValue?> FindAsync<TProjectedValue>(Expression<Func<TDomainObject, bool>> condition,
        CancellationToken cancellationToken = default) where TProjectedValue : DomainResume<TDomainObject>;
    Task<TDomainObject?> FindAsync<TKey>(Expression<Func<TDomainObject, bool>> condition,
        Expression<Func<TDomainObject, TKey>> orderBy, CancellationToken cancellationToken = default);
    Task<TDomainObject?> FindAsync(Expression<Func<TDomainObject, bool>> condition,
        IEnumerable<Expression<Func<TDomainObject, object>>> includes,
        CancellationToken cancellationToken = default);
    Task<TDomainObject?> FindLastAsync(Expression<Func<TDomainObject, bool>> condition,
        CancellationToken cancellationToken = default);
    Task<TDomainObject?> FindLastAsync<TKey>(Expression<Func<TDomainObject, bool>> condition,
        Expression<Func<TDomainObject, TKey>> orderBy, CancellationToken cancellationToken = default);
    Task<List<TDomainObject>> AllAsync(
        Expression<Func<TDomainObject, bool>> condition,
        CancellationToken cancellationToken);
    Task<List<TDomainObject>> AllAsync(
        Expression<Func<TDomainObject, bool>> condition,
        IEnumerable<Expression<Func<TDomainObject, object>>> includes,
        CancellationToken cancellationToken);
    Task<bool> AnyAsync(Expression<Func<TDomainObject, bool>> condition, CancellationToken cancellationToken = default);
    Task<long> CountAsync(Expression<Func<TDomainObject, bool>> condition, CancellationToken cancellationToken = default);
    IQueryable<TDomainObject> QueryAll();
    IQueryable<TDomainObject> QueryAll(
        Expression<Func<TDomainObject, bool>> condition);
    IQueryable<TDomainObject> QueryAll(
        Expression<Func<TDomainObject, bool>> condition, IEnumerable<Expression<Func<TDomainObject, object>>> includes);
}

public interface IDomainRepository
{
    
}