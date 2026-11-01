using BookingHub.Application.Common.Notifications;

namespace BookingHub.Infrastructure.IntegrationTests.TestDoubles;

/// <summary>For handler tests about bookings themselves, not about what the guest is told.</summary>
internal sealed class NoOpGuestMessages : IGuestMessages
{
    public Task BookingRequestedAsync(Booking firstBooking, int visitCount, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task BookingConfirmedAsync(Booking booking, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task BookingRescheduledAsync(Booking booking, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task BookingCancelledAsync(Booking booking, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task ReviewRequestedAsync(Booking booking, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task WaitlistSlotOfferedAsync(WaitlistEntry entry, CancellationToken cancellationToken) => Task.CompletedTask;
}