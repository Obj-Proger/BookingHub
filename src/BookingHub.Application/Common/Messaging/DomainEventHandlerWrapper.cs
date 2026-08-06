using Microsoft.Extensions.DependencyInjection;

namespace BookingHub.Application.Common.Messaging;

internal abstract class DomainEventHandlerWrapperBase
{
    public abstract Task Handle(IDomainEvent domainEvent, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}

internal sealed class DomainEventHandlerWrapper<TEvent> : DomainEventHandlerWrapperBase where TEvent : IDomainEvent
{
    public override async Task Handle(IDomainEvent domainEvent, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var typedEvent = (TEvent)domainEvent;

        foreach (var handler in serviceProvider.GetServices<IDomainEventHandler<TEvent>>())
            await handler.Handle(typedEvent, cancellationToken);
    }
}