namespace BookingHub.Application.Features.Locations.DTOs;

/// <param name="WorkingHours">One entry for each of the seven days of the week.</param>
public sealed record LocationResponse(
    Guid LocationId, string Name, string Address, string TimeZone, IReadOnlyList<DailyHoursDto> WorkingHours);