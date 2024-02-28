using Mediator;
using CognitoPOC.Domain.Common.Models;

namespace CognitoPOC.Domain.Core.Common.Queries;

public interface IQuery<TResult> : IRequest<OperationResultValue<TResult>>
{
}

public interface IPagedQuery<TResult> : IRequest<PagedResultValue<TResult>>
{
}

public interface ICollectionQuery<TResult> : IRequest<QueryResult<TResult>>
{
    
}