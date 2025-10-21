using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Modules.Common.Domain; // Needs access to IAuditableEntity
using System; // For DateTime
using System.Threading; // For CancellationToken
using System.Threading.Tasks; // For ValueTask

namespace Modules.Common.Infrastructure.Database;

public class AuditableInterceptor : SaveChangesInterceptor
{
    // Override SavingChangesAsync for async operations
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is not null)
        {
            UpdateAuditableEntities(context);
        }
        // Call the base method
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    // Override SavingChanges for sync operations (optional but good practice)
    public override InterceptionResult<int> SavingChanges(
       DbContextEventData eventData,
       InterceptionResult<int> result)
    {
        var context = eventData.Context;
        if (context is not null)
        {
            UpdateAuditableEntities(context);
        }
        return base.SavingChanges(eventData, result);
    }


    private static void UpdateAuditableEntities(DbContext context)
    {
        // Get all tracked entities that implement IAuditableEntity
        var entries = context.ChangeTracker.Entries<IAuditableEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = DateTime.UtcNow;
                entry.Entity.UpdatedAtUtc = null; // Ensure UpdatedAt is null on create
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
            }
        }
    }
}