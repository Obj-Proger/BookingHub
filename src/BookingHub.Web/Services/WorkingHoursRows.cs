using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Services;

/// <summary>An editable day of a location's weekly hours.</summary>
public sealed class WorkingHoursRow(DayOfWeek day)
{
    public DayOfWeek Day { get; } = day;
    public bool IsOpen { get; set; }
    public TimeOnly OpenTime { get; set; } = new(9, 0);
    public TimeOnly CloseTime { get; set; } = new(18, 0);
}

public static class WorkingHoursRows
{
    /// <summary>Monday to Friday 09:00–18:00, closed on weekends.</summary>
    public static List<WorkingHoursRow> CreateDefault() =>
        [.. WeekOrder.Days.Select(day => new WorkingHoursRow(day) { IsOpen = day is not (DayOfWeek.Saturday or DayOfWeek.Sunday) })];

    public static List<WorkingHoursRow> FromDtos(IReadOnlyList<DailyHoursDto> days) =>
        [.. WeekOrder.Days.Select(day =>
        {
            var row = new WorkingHoursRow(day);
            var dto = days.FirstOrDefault(d => d.DayOfWeek == day);

            if (dto?.OpenTime is { } openTime && dto.CloseTime is { } closeTime)
            {
                row.IsOpen = true;
                row.OpenTime = openTime;
                row.CloseTime = closeTime;
            }

            return row;
        })];

    public static List<DailyHoursDto> ToDtos(IEnumerable<WorkingHoursRow> rows) =>
        [.. rows.Select(row => row.IsOpen
            ? new DailyHoursDto(row.Day, row.OpenTime, row.CloseTime)
            : new DailyHoursDto(row.Day, null, null))];
}