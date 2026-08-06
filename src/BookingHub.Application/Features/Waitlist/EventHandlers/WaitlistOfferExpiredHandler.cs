using BookingHub.Application.Common.Messaging;
using BookingHub.Domain.Events;

namespace BookingHub.Application.Features.Waitlist.EventHandlers;

internal sealed class WaitlistOfferExpiredHandler(WaitlistOfferService offerService)
    : IDomainEventHandler<WaitlistOfferExpiredEvent>
{
    public Task Handle(WaitlistOfferExpiredEvent domainEvent, CancellationToken cancellationToken) =>
        offerService.TryOfferFreedSlotAsync(
            domainEvent.OrganizationId, domainEvent.LocationId, domainEvent.OfferedEmployeeId, domainEvent.ServiceId,
            domainEvent.OfferedSlot, cancellationToken);
}