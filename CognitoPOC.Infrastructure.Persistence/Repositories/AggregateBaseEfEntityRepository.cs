using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CognitoPOC.Domain.Common.Models;
using CognitoPOC.Infrastructure.Persistence.Contexts;

namespace CognitoPOC.Infrastructure.Persistence.Repositories;

public class AggregateBaseEfEntityRepository<TDomainEntity, TChildDomainEntity, TKey, TChildKey> 
    : BaseEfEntityRepository<TDomainEntity, TKey> 
    where TDomainEntity : AggregateDomainObject<TKey, TChildKey, TChildDomainEntity>
    where TChildDomainEntity : SecondaryDomainObject<TChildKey>
{
    protected readonly IQueryFilterTranslator<TChildDomainEntity, TChildKey>? ChildFilterTranslator;
    protected readonly DbSet<TChildDomainEntity> ChildDataSet;
    protected AggregateBaseEfEntityRepository(
        BaseEfDomainContext context, 
        IMapper? mapper = null, 
        IQueryFilterTranslator<TDomainEntity, TKey>? filterTranslator = null, 
        IQueryFilterTranslator<TChildDomainEntity, TChildKey>? childFilterTranslator = null) 
        : base(context, mapper, filterTranslator)
    {
        ChildFilterTranslator = childFilterTranslator;
        ChildDataSet = context.Set<TChildDomainEntity>();
    }
    protected virtual IQueryable<TChildDomainEntity> ChildGetQueryable
    {
        get
        {
            IQueryable<TChildDomainEntity> query = ChildDataSet;
            if (ReadOnly)
                query = query.AsNoTracking().OrderByDescending(p => p.CreatedDate);
            return query;
        }
    }
    protected async Task<IQueryable<TChildDomainEntity>> ChildBaseSearch(PagedFilteredRequestValue parameters,
        CancellationToken cancellationToken = default)
    {
        var query = ChildGetQueryable
            ;
        
        if (parameters.Filters != null && ChildFilterTranslator != null)
            query = await ChildFilterTranslator.AddFiltersAsync(query, parameters.Filters, cancellationToken);
        
        return query;
    }
}