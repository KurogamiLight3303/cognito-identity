using CognitoPOC.Domain.Common.Models;

namespace CognitoPOC.Domain.Core.Common.Queries;

public abstract class BasePagedQuery<TResult> : PagedFilteredRequestValue, IPagedQuery<TResult>
{
}