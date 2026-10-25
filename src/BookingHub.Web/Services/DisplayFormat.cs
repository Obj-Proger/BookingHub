using System.Globalization;

namespace BookingHub.Web.Services;

internal static class DisplayFormat
{
    public static string Price(decimal amount, string currency) =>
        $"{amount.ToString("0.00", CultureInfo.InvariantCulture)} {currency}";

    /// <summary>"45 min", "2 h" or "1 h 30 min"; sub-minute remainders are rounded.</summary>
    public static string Duration(TimeSpan duration)
    {
        var totalMinutes = (int)Math.Round(duration.TotalMinutes);
        var hours = totalMinutes / 60;
        var minutes = totalMinutes % 60;

        return hours == 0 ? $"{minutes} min"
            : minutes == 0 ? $"{hours} h"
            : $"{hours} h {minutes} min";
    }
}