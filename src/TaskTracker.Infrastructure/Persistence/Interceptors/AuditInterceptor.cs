using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TaskTracker.Application.Common.Interfaces;
using TaskTracker.Domain.Common;

namespace TaskTracker.Infrastructure.Persistence.Interceptors;

public sealed class AuditInterceptor(ICurrentUser currentUser, TimeProvider clock) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAudit(DbContext? context)
    {
        if (context is null) return;

        var now = clock.GetUtcNow();

        // The setters are private, so the values are written through EF, which also updates the entity instance.
        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(e => e.CreatedAt).CurrentValue = now;
                    entry.Property(e => e.UpdatedAt).CurrentValue = now;
                    entry.Property(e => e.CreatedBy).CurrentValue = currentUser.UserId;
                    break;

                case EntityState.Modified:
                    entry.Property(e => e.UpdatedAt).CurrentValue = now;
                    entry.Property(e => e.CreatedAt).IsModified = false;
                    entry.Property(e => e.CreatedBy).IsModified = false;
                    break;
            }
        }
    }
}