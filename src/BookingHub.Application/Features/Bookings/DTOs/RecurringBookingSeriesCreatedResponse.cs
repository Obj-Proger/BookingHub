namespace BookingHub.Application.Features.Bookings.DTOs;

public sealed record RecurringBookingSeriesCreatedResponse(
    Guid RecurringSeriesId,
    IReadOnlyList<BookingCreatedResponse> CreatedBookings,
    IReadOnlyList<DateTime> SkippedOccurrenceStartsUtc);