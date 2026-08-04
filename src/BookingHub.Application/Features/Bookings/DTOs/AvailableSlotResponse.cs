namespace BookingHub.Application.Features.Bookings.DTOs;

public sealed record AvailableSlotResponse(DateTime StartUtc, DateTime EndUtc);