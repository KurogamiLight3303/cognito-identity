using System.Linq.Expressions;
using System.Reflection;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CognitoPOC.Domain.Common.Models;
using CognitoPOC.Infrastructure.Persistence.Contexts;

namespace CognitoPOC.Infrastructure.Persistence.Repositories;

public abstract class BaseEfEntityRepository<TDomainEntity, TKey>(
    BaseEfDomainContext context,
    IMapper? mapper = null,
    IQueryFilterTranslator<TDomainEntity, TKey>? filterTranslator = null)
    where TDomainEntity : DomainObject<TKey>
{
    protected readonly IMapper? Mapper = mapper;
    protected readonly DbSet<TDomainEntity> DataSet = context.Set<TDomainEntity>();
    protected readonly BaseEfDomainContext Context = context;
    protected bool ReadOnly { get; init; }
    protected readonly IQueryFilterTranslator<TDomainEntity, TKey>? FilterTranslator = filterTranslator;

    #region "CRUD"
    public virtual async Task AddAsync(TDomainEntity entity, CancellationToken cancellationToken = default)
    {
        await DataSet.AddAsync(entity, cancellationToken);
    }
    public virtual void Update(TDomainEntity entity)
    {
        DataSet.Update(entity);
    }
    public virtual bool Update(TDomainEntity entity, Dictionary<Expression<Func<TDomainEntity, object>>, object> changes)
    {
        var result = false;
        foreach (var (key, value) in changes)
        {
            if (value is IComparable c && c.Equals(key.Compile().Invoke(entity))) continue;
            var memberExpression = key.Body as MemberExpression;
            var property = memberExpression?.Member as PropertyInfo;
            property?.SetValue(entity, value);
            result = true;
        }
        if(result)
            Update(entity);
        return result;
    }
    public void Remove(TDomainEntity entity)
    {
        DataSet.Remove(entity);
    }
    #endregion
    
    #region "Search"

    public virtual async Task<PagedResultValue<TProjectedValue>> SearchAsync<TProjectedValue>(PagedFilteredRequestValue parameters,
        Expression<Func<TDomainEntity, bool>> condition, CancellationToken cancellationToken = default)
    {
        if (Mapper == null)
            throw new Exception("Automapper not initialize");
        var sourceQuery = await BaseSearch(parameters, cancellationToken);
        sourceQuery = sourceQuery.Where(condition);
        
        var items = await Mapper.ProjectTo<TProjectedValue>(sourceQuery
            .Skip(parameters.PageSize * parameters.PageNo)
            .Take(parameters.PageSize)
        ).ToArrayAsync(cancellationToken);
        var count = await sourceQuery.LongCountAsync(cancellationToken);
        return new(items, parameters, count);
    }
    public async Task<PagedResultValue<TDomainEntity>> SearchAsync(PagedFilteredRequestValue parameters,
        CancellationToken cancellationToken = default)
    {
        var sourceQuery = await BaseSearch(parameters, cancellationToken);
        var items = await sourceQuery
            .Skip(parameters.PageSize * parameters.PageNo)
            .Take(parameters.PageSize)
            .ToArrayAsync(cancellationToken);
        var count = await sourceQuery.LongCountAsync(cancellationToken);
        return new(items, parameters, count);
    }
    public virtual async Task<PagedResultValue<TProjectedValue>> SearchAsync<TProjectedValue>(PagedFilteredRequestValue parameters,
        CancellationToken cancellationToken = default) where TProjectedValue : DomainResume<TDomainEntity>
    {
        if (Mapper == null)
            throw new Exception("Automapper not initialize");
        var sourceQuery = await BaseSearch(parameters, cancellationToken);
        var items = await Mapper.ProjectTo<TProjectedValue>(sourceQuery
            .Skip(parameters.PageSize * parameters.PageNo)
            .Take(parameters.PageSize)
        ).ToArrayAsync(cancellationToken);
        var count = await sourceQuery.LongCountAsync(cancellationToken);
        return new(items, parameters, count);
    }
    protected async Task<IQueryable<TDomainEntity>> BaseSearch(PagedFilteredRequestValue parameters,
        CancellationToken cancellationToken = default)
    {
        var query = GetQueryable
            ;
        
        if (parameters.Filters != null && FilterTranslator != null)
            query = await FilterTranslator.AddFiltersAsync(query, parameters.Filters, cancellationToken);

        if (parameters.Orders != null && FilterTranslator != null)
            query = await FilterTranslator.AddOrderAsync(query, parameters.Orders, cancellationToken);
        return query;
    }
    protected async Task<IQueryable<TEntity>> BaseSearch<TEntity, TEntityKey>(IQueryable<TEntity> query, 
        IQueryFilterTranslator<TEntity, TEntityKey> translator, PagedFilteredRequestValue parameters,
        CancellationToken cancellationToken = default) where TEntity : DomainObject<TEntityKey>
    {
        if (parameters.Filters != null && FilterTranslator != null)
            query = await translator.AddFiltersAsync(query, parameters.Filters, cancellationToken);
        
        if (parameters.Orders != null && FilterTranslator != null)
            query = await translator.AddOrderAsync(query, parameters.Orders, cancellationToken);
        
        return query;
    }
    #endregion

    #region "Find"
    public virtual async Task<TDomainEntity?> FindAsync(Expression<Func<TDomainEntity, bool>> condition, 
        CancellationToken cancellationToken = default)
    {
        return await GetQueryable.FirstOrDefaultAsync(condition, cancellationToken);
    }

    public virtual Task<TProjectedValue?> FindAsync<TProjectedValue>(Expression<Func<TDomainEntity, bool>> condition,
        CancellationToken cancellationToken = default) where TProjectedValue : DomainResume<TDomainEntity>
    {
        if (Mapper == null)
            throw new Exception("Automapper not initialize");
        return Mapper.ProjectTo<TProjectedValue>(GetQueryable.AsNoTracking().Where(condition)).FirstOrDefaultAsync(cancellationToken);
    }
    public virtual async Task<TDomainEntity?> FindAsync(Expression<Func<TDomainEntity, bool>> condition, 
        IEnumerable<Expression<Func<TDomainEntity, object>>> includes,
        CancellationToken cancellationToken = default)
    {
        var source = GetQueryable;
        foreach (var i in includes)
            source = source.Include(i);
        return await source.FirstOrDefaultAsync(condition, cancellationToken);
    }
    public async Task<TDomainEntity?> FindAsync<TProperty>(Expression<Func<TDomainEntity, bool>> condition, 
        Expression<Func<TDomainEntity, TProperty>> orderBy, CancellationToken cancellationToken = default)
    {
        return await GetQueryable.OrderBy(orderBy).FirstOrDefaultAsync(condition, cancellationToken);
    }
    public async Task<TDomainEntity?> FindLastAsync(Expression<Func<TDomainEntity, bool>> condition, 
        CancellationToken cancellationToken = default)
    {
        return await GetQueryable.LastOrDefaultAsync(condition, cancellationToken);
    }
    public async Task<TDomainEntity?> FindLastAsync<TProperty>(Expression<Func<TDomainEntity, bool>> condition, 
        Expression<Func<TDomainEntity, TProperty>> orderBy, CancellationToken cancellationToken = default)
    {
        return await GetQueryable.OrderBy(orderBy).LastOrDefaultAsync(condition, cancellationToken);
    }
    #endregion

    public async Task<bool> AnyAsync(Expression<Func<TDomainEntity, bool>> condition,
        CancellationToken cancellationToken = default)
    {
        return await GetQueryable.AnyAsync(condition, cancellationToken);
    }

    public async Task<long> CountAsync(Expression<Func<TDomainEntity, bool>> condition,
        CancellationToken cancellationToken = default)
    {
        return await GetQueryable.LongCountAsync(condition, cancellationToken);
    }
    public Task<List<TDomainEntity>> AllAsync(
        Expression<Func<TDomainEntity, bool>> condition,
        CancellationToken cancellationToken)
    {
        return GetQueryable.Where(condition).ToListAsync(cancellationToken);
    }

    public Task<List<TDomainEntity>> AllAsync(
        Expression<Func<TDomainEntity, bool>> condition,
        IEnumerable<Expression<Func<TDomainEntity, object>>> includes,
        CancellationToken cancellationToken)
    {
        var query = includes
            .Aggregate(GetQueryable, (current, i) => current.Include(i));
        return query.Where(condition).ToListAsync(cancellationToken);
    }

    public Task<List<TProjectedValue>> AllAsync<TProjectedValue>(
        Expression<Func<TDomainEntity, bool>> condition,
        CancellationToken cancellationToken)
        where TProjectedValue : DomainResume<TDomainEntity>
    {
        if (Mapper == null)
            throw new Exception("Automapper not initialize");
        return Mapper.ProjectTo<TProjectedValue>(GetQueryable.Where(condition)).ToListAsync(cancellationToken);
    }

    public Task<List<TProjectedValue>> AllAsync<TProjectedValue>(
        CancellationToken cancellationToken)
        where TProjectedValue : DomainResume<TDomainEntity>
    {
        if (Mapper == null)
            throw new Exception("Automapper not initialize");
        return Mapper.ProjectTo<TProjectedValue>(GetQueryable).ToListAsync(cancellationToken);
    }

    public IQueryable<TProjectedValue> QueryAll<TProjectedValue>(
        Expression<Func<TDomainEntity, bool>> condition)
        where TProjectedValue : DomainResume<TDomainEntity>
    {
        if (Mapper == null)
            throw new Exception("Automapper not initialize");
        return Mapper.ProjectTo<TProjectedValue>(GetQueryable.Where(condition));
    }
    public IQueryable<TProjectedValue> QueryAll<TProjectedValue>(
        Expression<Func<TDomainEntity, bool>> condition, Expression<Func<TDomainEntity, object>> orderBy)
        where TProjectedValue : DomainResume<TDomainEntity>
    {
        if (Mapper == null)
            throw new Exception("Automapper not initialize");
        return Mapper.ProjectTo<TProjectedValue>(GetQueryable.Where(condition).OrderBy(orderBy));
    }
    public IQueryable<TProjectedValue> QueryAll<TProjectedValue>()
        where TProjectedValue : DomainResume<TDomainEntity>
    {
        if (Mapper == null)
            throw new Exception("Automapper not initialize");
        return Mapper.ProjectTo<TProjectedValue>(GetQueryable);
    }

    public IQueryable<TDomainEntity> QueryAll()
    {
        return GetQueryable;
    }

    public IQueryable<TDomainEntity> QueryAll(
        Expression<Func<TDomainEntity, bool>> condition)
    {
        return GetQueryable.Where(condition);
    }

    public IQueryable<TDomainEntity> QueryAll(
        Expression<Func<TDomainEntity, bool>> condition, IEnumerable<Expression<Func<TDomainEntity, object>>> includes)
    {
        var source = includes.Aggregate(GetQueryable,
            (current, i) => current.Include(i));
        return source.Where(condition);
    }

    protected virtual IQueryable<TDomainEntity> GetQueryable
    {
        get
        {
            IQueryable<TDomainEntity> query = DataSet;
            if (ReadOnly)
                query = query.AsNoTracking().OrderByDescending(p => p.CreatedDate);
            return query;
        }
    }
}