using BookingHub.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BookingHub.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Stamps <see cref="IAuditable"/> timestamps just before every save — the only place in the
/// solution allowed to do so (see IAuditable itself: entities expose private setters, reachable
/// here only through EF Core's ChangeTracker, not through ordinary C# member access).
/// </summary>
internal sealed class AuditableEntitySaveChangesInterceptor(TimeProvider timeProvider) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        StampAuditableEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        StampAuditableEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void StampAuditableEntities(DbContext? context)
    {
        if (context is null)
            return;

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        foreach (var entry in context.ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State == EntityState.Added)
                entry.Property(nameof(IAuditable.CreatedAtUtc)).CurrentValue = utcNow;

            if (entry.State is EntityState.Added or EntityState.Modified)
                entry.Property(nameof(IAuditable.ModifiedAtUtc)).CurrentValue = utcNow;
        }
    }
}