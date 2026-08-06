namespace BookingHub.Application.Common.Messaging;

/// <summary>
/// Fans a domain event out to zero or more <see cref="IDomainEventHandler{TEvent}"/> subscribers.
/// Unlike <see cref="IDispatcher"/> (exactly one handler per request), any number of handlers —
/// including none — may exist for a given event type.
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken);
}