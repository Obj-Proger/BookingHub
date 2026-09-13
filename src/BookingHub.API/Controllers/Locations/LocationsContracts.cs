using BookingHub.Application.Features.Locations.DTOs;

namespace BookingHub.API.Controllers.Locations;

public sealed record CreateLocationRequest(string? Name, string? Address, string? TimeZone, IReadOnlyList<DailyHoursDto> WorkingHours);
public sealed record RenameLocationRequest(string? NewName);
public sealed record UpdateLocationAddressRequest(string? NewAddress);
public sealed record UpdateLocationWorkingHoursRequest(IReadOnlyList<DailyHoursDto> WorkingHours);