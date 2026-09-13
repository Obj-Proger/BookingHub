namespace BookingHub.API.Controllers.Bookings;

public sealed record CreateBookingRequest(
    Guid LocationId, Guid EmployeeId, Guid ServiceId, DateTime StartUtc, string? Phone, string? ClientName, string? ClientEmail);

public sealed record CreateRecurringBookingSeriesRequest(
    Guid LocationId, Guid EmployeeId, Guid ServiceId, DateTime FirstStartUtc, int IntervalWeeks, int OccurrenceCount,
    string? Phone, string? ClientName, string? ClientEmail);

public sealed record ConfirmBookingRequest(string? Token);
public sealed record CancelBookingRequest(string? Token, string? Reason);
public sealed record RescheduleBookingRequest(string? Token, DateTime NewStartUtc);