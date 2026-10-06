using BookingHub.Domain.Enums;

namespace BookingHub.Application.Features.Bookings.DTOs;

public sealed record PublicBookingDetailsResponse(
    Guid BookingId, Guid LocationId, Guid EmployeeId, Guid ServiceId, string ServiceName,
    DateTime StartUtc, DateTime EndUtc, BookingStatus Status);