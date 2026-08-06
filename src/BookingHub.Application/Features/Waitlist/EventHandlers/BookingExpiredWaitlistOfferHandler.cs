using BookingHub.Application.Common.Messaging;
using BookingHub.Domain.Events;

namespace BookingHub.Application.Features.Waitlist.EventHandlers;

internal sealed class BookingExpiredWaitlistOfferHandler(IWaitlistOfferService offerService)
    : IDomainEventHandler<BookingExpiredEvent>
{
    public Task Handle(BookingExpiredEvent domainEvent, CancellationToken cancellationToken) =>
        offerService.TryOfferFreedSlotAsync(
            domainEvent.OrganizationId, domainEvent.LocationId, domainEvent.EmployeeId, domainEvent.ServiceId,
            domainEvent.TimeSlot, cancellationToken);
}