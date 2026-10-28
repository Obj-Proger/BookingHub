namespace BookingHub.Web.Localization;

internal static class SupportedCultures
{
    public const string Default = "en";

    public static readonly string[] All = ["en", "ru"];

    public static bool IsSupported(string? culture) => All.Contains(culture, StringComparer.OrdinalIgnoreCase);
}