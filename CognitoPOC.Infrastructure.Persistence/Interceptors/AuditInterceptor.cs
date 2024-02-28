using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using CognitoPOC.Domain.Common.Models;
using CognitoPOC.Domain.Services;

namespace CognitoPOC.Infrastructure.Persistence.Interceptors;

public class AuditInterceptor(ILogger logger, ISessionMetadataService metadataService) : ISaveChangesInterceptor
{
    #region SavingChanges
    public ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateAudit(eventData.Context!.ChangeTracker.Entries());
        return ValueTask.FromResult(result);
    }

    public InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateAudit(eventData.Context!.ChangeTracker.Entries());
        return result;
    }

    private void UpdateAudit(IEnumerable<EntityEntry> entries)
    {
        try
        {
            var username = metadataService.GetCurrentUsername();
            var date = DateTime.Now;
            foreach (var e in entries)
            {
                if (e.Entity is DomainObject d)
                {
                    switch (e.State)
                    {
                        case EntityState.Added:
                            d.UpdatedDate = d.CreatedDate = date;
                            d.UpdatedBy = d.CreatedBy = username;
                            break;
                        case EntityState.Modified:
                            d.UpdatedBy = username;
                            d.UpdatedDate = date;
                            break;
                    }
                }
            }
        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Unable to update audit log");
        }
    }
    #endregion
}