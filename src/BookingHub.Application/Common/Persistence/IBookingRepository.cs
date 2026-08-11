using BookingHub.Domain.Entities;

namespace BookingHub.Application.Common.Persistence;

public interface IBookingRepository
{
    void Add(Booking booking);

    Task<Booking?> GetByIdAsync(Guid bookingId, CancellationToken cancellationToken);

    /// <param name="organizationId">Filtered alongside <paramref name="locationId"/>, <paramref name="employeeId"/>,
    /// and <paramref name="bookingId"/> together — staff-side access, unlike the token-based guest flows.</param>
    Task<Booking?> GetByIdAsync(
        Guid organizationId, Guid locationId, Guid employeeId, Guid bookingId, CancellationToken cancellationToken);

    /// <summary>Pending bookings whose owning organization's confirmation window has elapsed.</summary>
    Task<IReadOnlyList<Booking>> GetPendingBookingsPastConfirmationWindowAsync(DateTime utcNow, CancellationToken cancellationToken);

    /// <summary>Confirmed bookings whose time slot has already ended.</summary>
    Task<IReadOnlyList<Booking>> GetConfirmedBookingsWithEndedSlotsAsync(DateTime utcNow, CancellationToken cancellationToken);

    /// <summary>AwaitingReview bookings whose owning organization's auto-complete window has elapsed.</summary>
    Task<IReadOnlyList<Booking>> GetAwaitingReviewBookingsPastAutoCompleteWindowAsync(DateTime utcNow, CancellationToken cancellationToken);

    /// <summary>Other Pending bookings in the same recurring series — for cascading confirmation
    /// when the guest confirms via the first occurrence's link.</summary>
    Task<IReadOnlyList<Booking>> GetPendingSiblingsInSeriesAsync(
        Guid recurringSeriesId, Guid excludingBookingId, CancellationToken cancellationToken);
}