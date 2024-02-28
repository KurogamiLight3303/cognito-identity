using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace CognitoPOC.Infrastructure.Persistence.Contexts;

public class DomainContext(DbContextOptions<DomainContext> options) : BaseEfDomainContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}