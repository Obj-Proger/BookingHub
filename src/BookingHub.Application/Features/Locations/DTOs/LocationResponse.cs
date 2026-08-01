namespace BookingHub.Application.Features.Locations.DTOs;

public sealed record LocationResponse(Guid LocationId, string Name, string Address, string TimeZone);