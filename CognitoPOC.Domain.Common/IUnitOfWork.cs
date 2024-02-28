namespace CognitoPOC.Domain.Common;

public interface IUnitOfWork
{
    Task<TResult> ExecuteInTransactionAsync<TResult>(Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken);
    Task<bool> CommitAsync(CancellationToken cancellationToken);
}