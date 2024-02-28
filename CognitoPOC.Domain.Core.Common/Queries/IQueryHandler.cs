using Mediator;
using CognitoPOC.Domain.Common.Models;

namespace CognitoPOC.Domain.Core.Common.Queries;

public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, OperationResultValue<TResult>> 
    where TQuery : IQuery<TResult>
{
}

public interface IPagedQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, PagedResultValue<TResult>>
    where TQuery : IPagedQuery<TResult>
{
}

public interface ICollectionQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, QueryResult<TResult>>
    where TQuery : ICollectionQuery<TResult>
{
    
}