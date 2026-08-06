using System.Collections.Concurrent;

namespace BookingHub.Application.Common.Messaging;

internal sealed class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    private static readonly ConcurrentDictionary<Type, DomainEventHandlerWrapperBase> Wrappers = new();

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in domainEvents)
        {
            var eventType = domainEvent.GetType();

            var wrapper = Wrappers.GetOrAdd(eventType, type =>
                (DomainEventHandlerWrapperBase)Activator.CreateInstance(typeof(DomainEventHandlerWrapper<>).MakeGenericType(type))!);

            await wrapper.Handle(domainEvent, serviceProvider, cancellationToken);
        }
    }
}