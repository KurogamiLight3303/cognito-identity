namespace CognitoPOC.Domain.Common.Models;

public interface IQueryFilterTranslator<TDomainObject, TKey> : IQueryFilterTranslator 
    where TDomainObject : DomainObject<TKey>
{
    Task<IQueryable<TDomainObject>> AddFiltersAsync(IQueryable<TDomainObject> query, 
        IEnumerable<QueryFilterValue> filters, CancellationToken cancellationToken);
    Task<IQueryable<TDomainObject>> AddOrderAsync(IQueryable<TDomainObject> query, 
        IEnumerable<QueryOrderValue> filters, CancellationToken cancellationToken);
}

public interface IQueryFilterTranslator
{
    
}