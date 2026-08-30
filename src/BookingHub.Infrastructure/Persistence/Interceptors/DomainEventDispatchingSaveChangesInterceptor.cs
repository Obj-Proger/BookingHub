using BookingHub.Application.Common.Messaging;
using BookingHub.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace BookingHub.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Dispatches domain events only from <see cref="SavedChangesAsync"/> — the post-commit hook —
/// never from <c>SavingChanges</c>: per IDomainEvent's own contract (Domain), an event means the
/// fact it describes is already durable, so subscribers must never react to state that could
/// still be rolled back.
/// </summary>
internal sealed class DomainEventDispatchingSaveChangesInterceptor(
    IDomainEventDispatcher domainEventDispatcher, ILogger<DomainEventDispatchingSaveChangesInterceptor> logger)
    : SaveChangesInterceptor
{
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            await DispatchDomainEventsAsync(eventData.Context, cancellationToken);

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private async Task DispatchDomainEventsAsync(DbContext context, CancellationToken cancellationToken)
    {
        var entitiesWithEvents = context.ChangeTracker.Entries<IHasDomainEvents>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Count > 0)
            .ToList();

        var domainEvents = entitiesWithEvents.SelectMany(e => e.DomainEvents).ToList();

        foreach (var entity in entitiesWithEvents)
            entity.ClearDomainEvents();

        if (domainEvents.Count == 0)
            return;

        foreach (var domainEvent in domainEvents)
            logger.LogInformation("Dispatching domain event {DomainEventType}", domainEvent.GetType().Name);

        try
        {
            await domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Domain event dispatch failed for: {EventTypes}",
                string.Join(", ", domainEvents.Select(e => e.GetType().Name)));
        }
    }
}