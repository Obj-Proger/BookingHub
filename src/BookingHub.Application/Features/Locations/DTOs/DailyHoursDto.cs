namespace BookingHub.Application.Features.Locations.DTOs;

public sealed record DailyHoursDto(DayOfWeek DayOfWeek, TimeOnly? OpenTime, TimeOnly? CloseTime);