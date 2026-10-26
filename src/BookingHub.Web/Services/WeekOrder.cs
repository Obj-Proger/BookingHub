namespace BookingHub.Web.Services;

internal static class WeekOrder
{
    /// <summary>The days of the week as they are shown in the UI: Monday first.</summary>
    public static readonly DayOfWeek[] Days =
    [
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
        DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
    ];
}