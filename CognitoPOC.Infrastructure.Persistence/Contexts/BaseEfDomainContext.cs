using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace CognitoPOC.Infrastructure.Persistence.Contexts;

public abstract class BaseEfDomainContext : DbContext
{
    protected BaseEfDomainContext()
    {
        
    }
    protected BaseEfDomainContext(DbContextOptions options) : base(options)
    {
        
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}