namespace BookingHub.Web.Api.Contracts;

public enum BookingStatus
{
    Pending = 1,
    Confirmed = 2,
    AwaitingReview = 3,
    Completed = 4,
    NoShow = 5,
    Cancelled = 6,
    Expired = 7
}

public sealed record AvailableSlotResponse(DateTime StartUtc, DateTime EndUtc);

public sealed record CreateBookingRequest(
    Guid LocationId, Guid EmployeeId, Guid ServiceId, DateTime StartUtc, string? Phone, string? ClientName, string? ClientEmail);

public sealed record BookingCreatedResponse(Guid BookingId, DateTime StartUtc, DateTime EndUtc, BookingStatus Status);