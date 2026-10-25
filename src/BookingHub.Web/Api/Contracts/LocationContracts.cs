namespace BookingHub.Web.Api.Contracts;

public sealed record DailyHoursDto(DayOfWeek DayOfWeek, TimeOnly? OpenTime, TimeOnly? CloseTime);

public sealed record LocationResponse(
    Guid LocationId, string Name, string Address, string TimeZone, IReadOnlyList<DailyHoursDto> WorkingHours);

public sealed record LocationCreatedResponse(Guid LocationId, string Name);

public sealed record CreateLocationRequest(
    string? Name, string? Address, string? TimeZone, IReadOnlyList<DailyHoursDto> WorkingHours);

public sealed record RenameLocationRequest(string? NewName);

public sealed record UpdateLocationAddressRequest(string? NewAddress);

public sealed record UpdateLocationWorkingHoursRequest(IReadOnlyList<DailyHoursDto> WorkingHours);