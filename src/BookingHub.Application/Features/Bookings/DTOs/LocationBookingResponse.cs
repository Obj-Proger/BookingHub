using BookingHub.Domain.Enums;

namespace BookingHub.Application.Features.Bookings.DTOs;

public sealed record LocationBookingResponse(
    Guid BookingId, Guid EmployeeId, string EmployeeName, DateTime StartUtc, DateTime EndUtc,
    string ServiceName, string? ClientName, string ClientPhone, BookingStatus Status);