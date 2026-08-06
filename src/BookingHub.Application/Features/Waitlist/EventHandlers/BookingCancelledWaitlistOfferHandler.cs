using BookingHub.Application.Common.Messaging;
using BookingHub.Domain.Events;

namespace BookingHub.Application.Features.Waitlist.EventHandlers;

internal sealed class BookingCancelledWaitlistOfferHandler(WaitlistOfferService offerService)
    : IDomainEventHandler<BookingCancelledEvent>
{
    public Task Handle(BookingCancelledEvent domainEvent, CancellationToken cancellationToken) =>
        offerService.TryOfferFreedSlotAsync(
            domainEvent.OrganizationId, domainEvent.LocationId, domainEvent.EmployeeId, domainEvent.ServiceId,
            domainEvent.TimeSlot, cancellationToken);
}