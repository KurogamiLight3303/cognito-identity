using Microsoft.Extensions.Logging;
using CognitoPOC.Infrastructure.Persistence.Contexts;

namespace CognitoPOC.Infrastructure.Persistence.UnitsOfWork;

public class DomainUnitOfWork : BaseEfUnitOfWork
{
    public DomainUnitOfWork(DomainContext context, ILogger logging) : base(context, logging)
    {
    }
}