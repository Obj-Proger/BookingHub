using BookingHub.Domain.Entities;

namespace BookingHub.Application.Common.Notifications;

/// <summary>
/// Every message a guest receives, worded in one place. Each method queues its message through
/// <see cref="IGuestNotifier"/>, so it must be called before the unit of work is saved.
/// </summary>
internal interface IGuestMessages
{
    /// <summary>Asks the guest to confirm a new booking — or a whole series, when <paramref name="visitCount"/> is above one.</summary>
    Task BookingRequestedAsync(Booking firstBooking, int visitCount, CancellationToken cancellationToken);

    /// <summary>Confirms the booking and sends the link to manage (cancel or reschedule) it.</summary>
    Task BookingConfirmedAsync(Booking booking, CancellationToken cancellationToken);

    Task BookingRescheduledAsync(Booking booking, CancellationToken cancellationToken);

    Task BookingCancelledAsync(Booking booking, CancellationToken cancellationToken);

    /// <summary>Thanks the guest for a completed visit and asks for a review.</summary>
    Task ReviewRequestedAsync(Booking booking, CancellationToken cancellationToken);

    /// <summary>Offers the guest the slot their waitlist entry now holds, until the offer expires.</summary>
    Task WaitlistSlotOfferedAsync(WaitlistEntry entry, CancellationToken cancellationToken);
}