using BookingHub.Mobile.Domain;

namespace BookingHub.Mobile.Api.Contracts;

public sealed record EmployeeBookingResponse(
    Guid BookingId, DateTime StartUtc, DateTime EndUtc, string ServiceName,
    string? ClientName, string ClientPhone, BookingStatus Status);