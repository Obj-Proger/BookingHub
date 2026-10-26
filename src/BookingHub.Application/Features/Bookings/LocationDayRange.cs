namespace BookingHub.Application.Features.Bookings;

internal static class LocationDayRange
{
    /// <summary>The UTC interval [start, end) that a calendar date covers in the given time zone.</summary>
    public static (DateTime StartUtc, DateTime EndUtc) ToUtc(string timeZoneId, DateOnly date)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

        return (
            TimeZoneInfo.ConvertTimeToUtc(date.ToDateTime(TimeOnly.MinValue), timeZone),
            TimeZoneInfo.ConvertTimeToUtc(date.AddDays(1).ToDateTime(TimeOnly.MinValue), timeZone));
    }
}