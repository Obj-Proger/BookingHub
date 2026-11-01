using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Notifications;
using BookingHub.Application.Common.Persistence;
using BookingHub.Domain.Enums;
using BookingHub.Domain.Events;

namespace BookingHub.Application.Features.Waitlist.EventHandlers;

/// <summary>
/// Tells the guest about the offer. The event is handled after the offer was committed, possibly
/// seconds later, so the entry is read again: an offer that has since been accepted, cancelled or
/// expired is not announced.
/// </summary>
internal sealed class WaitlistSlotOfferedNotificationHandler(
    IWaitlistEntryRepository waitlistEntryRepository, IGuestMessages guestMessages)
    : IDomainEventHandler<WaitlistSlotOfferedEvent>
{
    public async Task Handle(WaitlistSlotOfferedEvent domainEvent, CancellationToken cancellationToken)
    {
        var entry = await waitlistEntryRepository.GetByIdAsync(domainEvent.WaitlistEntryId, cancellationToken);
        if (entry is null || entry.Status != WaitlistEntryStatus.Offered)
            return;

        // Saved by the outbox processor together with marking this event processed.
        await guestMessages.WaitlistSlotOfferedAsync(entry, cancellationToken);
    }
}