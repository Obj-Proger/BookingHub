using System.Text.RegularExpressions;

namespace BookingHub.Web.Services;

internal static partial class SlugSuggestion
{
    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonSlugCharacters();

    /// <summary>Lowercase ASCII letters and digits joined by hyphens; empty if the name has none.</summary>
    public static string FromName(string? name) =>
        NonSlugCharacters().Replace((name ?? string.Empty).Trim().ToLowerInvariant(), "-").Trim('-');
}