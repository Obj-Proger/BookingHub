using BookingHub.Domain.Enums;

namespace BookingHub.Application.Features.Bookings.DTOs;

public sealed record EmployeeBookingResponse(
    Guid BookingId, DateTime StartUtc, DateTime EndUtc, string ServiceName,
    string? ClientName, string ClientPhone, BookingStatus Status);