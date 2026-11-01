using System.Collections.Concurrent;
using BookingHub.Application.Common.Messaging;
using BookingHub.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace BookingHub.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Moves the domain events raised by tracked entities into the outbox as part of the same save, so
/// an event is stored if and only if the change that raised it is. <see cref="OutboxProcessor"/>
/// hands it to its handlers after the commit. Events nobody subscribes to are not stored at all.
/// </summary>
internal sealed class DomainEventOutboxInterceptor(IServiceProviderIsService serviceProviderIsService) : SaveChangesInterceptor
{
    private readonly ConcurrentDictionary<Type, bool> _hasHandlers = new();

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        MoveDomainEventsToOutbox(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        MoveDomainEventsToOutbox(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void MoveDomainEventsToOutbox(DbContext? context)
    {
        if (context is null)
            return;

        var entitiesWithEvents = context.ChangeTracker.Entries<IHasDomainEvents>()
            .Select(entry => entry.Entity)
            .Where(entity => entity.DomainEvents.Count > 0)
            .ToList();

        foreach (var entity in entitiesWithEvents)
        {
            foreach (var domainEvent in entity.DomainEvents.Where(HasHandlers))
            {
                context.Set<OutboxMessage>().Add(
                    OutboxMessage.Create(domainEvent.OrganizationId, domainEvent, domainEvent.OccurredOnUtc));
            }

            entity.ClearDomainEvents();
        }
    }

    private bool HasHandlers(IDomainEvent domainEvent) =>
        _hasHandlers.GetOrAdd(domainEvent.GetType(), eventType =>
            serviceProviderIsService.IsService(typeof(IDomainEventHandler<>).MakeGenericType(eventType)));
}