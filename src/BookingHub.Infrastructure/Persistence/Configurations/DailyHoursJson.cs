using BookingHub.Domain.ValueObjects;

namespace BookingHub.Infrastructure.Persistence.Configurations;

/// <summary>
/// Pure serialization shape for WeeklyHours/DailyHours — no domain behavior, exists only so
/// System.Text.Json has a plain, publicly-constructible type to (de)serialize; reconstruction
/// always goes through the real DailyHours/WeeklyHours factories below, never bypassing them.
/// </summary>
internal sealed record DailyHoursJson(int DayOfWeek, TimeOnly? OpenTime, TimeOnly? CloseTime)
{
    public static DailyHoursJson FromDomain(DailyHours dailyHours) =>
        new((int)dailyHours.DayOfWeek, dailyHours.OpenTime, dailyHours.CloseTime);

    public DailyHours ToDomain() =>
        OpenTime is null || CloseTime is null
            ? DailyHours.CreateClosed((DayOfWeek)DayOfWeek)
            : DailyHours.CreateOpen((DayOfWeek)DayOfWeek, OpenTime.Value, CloseTime.Value).Value;
}