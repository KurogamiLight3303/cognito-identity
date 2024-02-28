using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using CognitoPOC.Domain.Common;
using CognitoPOC.Infrastructure.Persistence.Contexts;

namespace CognitoPOC.Infrastructure.Persistence.UnitsOfWork;

public class BaseEfUnitOfWork : IUnitOfWork
{
    private readonly BaseEfDomainContext _context;
    private readonly ILogger _logging;
    private IDbContextTransaction? _transaction;

    protected BaseEfUnitOfWork(BaseEfDomainContext context, ILogger logging)
    {
        _context = context;
        _logging = logging;
    }

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<CancellationToken, Task<TResult>> operation, 
        CancellationToken cancellationToken)
    {
        if (_transaction != null)
            throw new InvalidOperationException("Already in transaction");
        return await _context.Database.CreateExecutionStrategy().ExecuteAsync(async (cc) =>
        {
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            return await operation.Invoke(cc);
        }, cancellationToken);
    }

    public async Task<bool> CommitAsync(CancellationToken cancellationToken)
    {
        var result = false;
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
            result = true;
        }
        catch(Exception exc)
        {
            _logging.LogError(exc, "Commit Error");
        }

        return result;
    }
}