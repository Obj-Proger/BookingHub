using System.Text.RegularExpressions;

namespace BookingHub.API.Tests;

/// <summary>A link from a guest message or the seed output: /book/{slug}/{page}/{id}?token={token}.</summary>
public sealed partial record GuestLink(Guid Id, string Token)
{
    [GeneratedRegex(@"/book/[^/\s]+/(?<page>[a-z]+)/(?<id>[0-9a-f-]{36})\?token=(?<token>[^\s&]+)")]
    private static partial Regex Pattern();

    /// <summary>The first <paramref name="page"/> link in <paramref name="text"/>, or null when there is none.</summary>
    public static GuestLink? Find(string text, string page) =>
        Pattern().Matches(text)
            .Where(match => match.Groups["page"].Value == page)
            .Select(match => new GuestLink(Guid.Parse(match.Groups["id"].Value), match.Groups["token"].Value))
            .FirstOrDefault();
}
