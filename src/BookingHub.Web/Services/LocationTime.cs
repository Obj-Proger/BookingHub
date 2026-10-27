namespace BookingHub.Web.Services;

/// <summary>
/// Converts between UTC instants and the wall-clock time of a single location.
/// The Web host runs on the server, so its own local time zone is never the guest's or the location's.
/// </summary>
public sealed class LocationTime
{
    // The API rejects a waitlist window that starts before its own "now", so the window
    // never starts earlier than this margin ahead of the Web host's clock.
    private static readonly TimeSpan WindowStartMargin = TimeSpan.FromMinutes(1);

    private readonly TimeZoneInfo _zone;

    private LocationTime(TimeZoneInfo zone) => _zone = zone;

    public string ZoneId => _zone.Id;

    /// <summary>Falls back to UTC when the zone id is unknown on this host.</summary>
    public static LocationTime FromZoneId(string zoneId) =>
        new(TimeZoneInfo.TryFindSystemTimeZoneById(zoneId, out var zone) ? zone : TimeZoneInfo.Utc);

    public DateTime ToLocal(DateTime utc)
    {
        var normalized = utc.Kind == DateTimeKind.Local
            ? utc.ToUniversalTime()
            : DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(normalized, _zone);
    }

    public DateOnly Today(TimeProvider clock) =>
        DateOnly.FromDateTime(ToLocal(clock.GetUtcNow().UtcDateTime));

    /// <summary>The UTC instant at which the given local calendar date begins.</summary>
    public DateTime StartOfDayUtc(DateOnly date) => LocalMidnightToUtc(date);

    /// <summary>
    /// The part of the given local day that is still ahead, as a half-open UTC interval:
    /// from the later of local midnight and now (plus a small margin) until the next local midnight.
    /// </summary>
    public (DateTime StartUtc, DateTime EndUtc) GetRemainingDayWindowUtc(DateOnly date, TimeProvider clock)
    {
        var dayStartUtc = LocalMidnightToUtc(date);
        var dayEndUtc = LocalMidnightToUtc(date.AddDays(1));
        var earliestUtc = clock.GetUtcNow().UtcDateTime + WindowStartMargin;

        return (dayStartUtc > earliestUtc ? dayStartUtc : earliestUtc, dayEndUtc);
    }

    private DateTime LocalMidnightToUtc(DateOnly date)
    {
        var local = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);

        // Some zones skip local midnight on DST transition days.
        if (_zone.IsInvalidTime(local))
            local = local.AddHours(1);

        return TimeZoneInfo.ConvertTimeToUtc(local, _zone);
    }
}