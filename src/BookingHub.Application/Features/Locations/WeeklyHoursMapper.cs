using BookingHub.Application.Features.Locations.DTOs;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Application.Features.Locations;

/// <summary>Converts the wire-friendly <see cref="DailyHoursDto"/> list into a validated <see cref="WeeklyHours"/>.</summary>
internal static class WeeklyHoursMapper
{
    public static Result<WeeklyHours> ToDomain(IReadOnlyList<DailyHoursDto> workingHours)
    {
        var dailyHours = new List<DailyHours>();

        foreach (var dto in workingHours)
        {
            if (dto.OpenTime is null || dto.CloseTime is null)
            {
                dailyHours.Add(DailyHours.CreateClosed(dto.DayOfWeek));
                continue;
            }

            var dailyResult = DailyHours.CreateOpen(dto.DayOfWeek, dto.OpenTime.Value, dto.CloseTime.Value);
            if (dailyResult.IsFailure)
                return Result.Failure<WeeklyHours>(dailyResult.Error);

            dailyHours.Add(dailyResult.Value);
        }

        return WeeklyHours.Create(dailyHours);
    }
}