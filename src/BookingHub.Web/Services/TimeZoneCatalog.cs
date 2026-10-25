namespace BookingHub.Web.Services;

internal sealed record TimeZoneOption(string Id, string Label);

/// <summary>The time zones this host knows, identified by IANA id regardless of the host's operating system.</summary>
internal static class TimeZoneCatalog
{
    private static readonly Lazy<IReadOnlyList<ZoneEntry>> Zones = new(LoadZones);

    /// <summary>Options ordered by current UTC offset, then id; offsets are as of <paramref name="now"/>.</summary>
    public static IReadOnlyList<TimeZoneOption> GetOptions(DateTimeOffset now) =>
        [.. Zones.Value
            .Select(entry => (entry.IanaId, Offset: entry.Zone.GetUtcOffset(now)))
            .OrderBy(entry => entry.Offset)
            .ThenBy(entry => entry.IanaId, StringComparer.Ordinal)
            .Select(entry => new TimeZoneOption(entry.IanaId, $"{entry.IanaId} (UTC{FormatOffset(entry.Offset)})"))];

    private static IReadOnlyList<ZoneEntry> LoadZones() =>
        [.. TimeZoneInfo.GetSystemTimeZones()
            .Select(zone => (IanaId: ToIanaId(zone), Zone: zone))
            .Where(entry => entry.IanaId is not null)
            .DistinctBy(entry => entry.IanaId)
            .Select(entry => new ZoneEntry(entry.IanaId!, entry.Zone))];

    private static string? ToIanaId(TimeZoneInfo zone) =>
        zone.HasIanaId
            ? zone.Id
            : TimeZoneInfo.TryConvertWindowsIdToIanaId(zone.Id, out var ianaId) ? ianaId : null;

    private static string FormatOffset(TimeSpan offset) =>
        (offset < TimeSpan.Zero ? "-" : "+") + offset.ToString(@"hh\:mm");

    private sealed record ZoneEntry(string IanaId, TimeZoneInfo Zone);
}