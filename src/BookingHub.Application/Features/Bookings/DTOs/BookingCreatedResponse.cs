using BookingHub.Domain.Enums;

namespace BookingHub.Application.Features.Bookings.DTOs;

public sealed record BookingCreatedResponse(Guid BookingId, DateTime StartUtc, DateTime EndUtc, BookingStatus Status);