namespace BookingHub.Web.Auth;

internal static class LocalUrl
{
    /// <summary>Returns <paramref name="url"/> only if it is a site-relative path; otherwise the fallback.</summary>
    public static string OrDefault(string? url, string fallback) => IsLocal(url) ? url! : fallback;

    private static bool IsLocal(string? url) =>
        !string.IsNullOrEmpty(url)
        && url[0] == '/'
        && (url.Length == 1 || (url[1] != '/' && url[1] != '\\'))
        && !url.Any(char.IsControl);
}