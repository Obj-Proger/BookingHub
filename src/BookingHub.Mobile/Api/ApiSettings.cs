using System.Reflection;

namespace BookingHub.Mobile.Api;

internal static class ApiSettings
{
    private const string MetadataKey = "ApiBaseUrl";

    /// <summary>
    /// The API this build talks to, fixed at build time by the ApiBaseUrl MSBuild property (see the
    /// csproj for the local-development defaults), e.g.
    /// <c>dotnet build -f net10.0-android -p:ApiBaseUrl=https://api.example.com/</c>.
    /// </summary>
    public static Uri BaseUrl { get; } = ReadBaseUrl();

    private static Uri ReadBaseUrl()
    {
        var value = typeof(ApiSettings).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(attribute => attribute.Key == MetadataKey)?.Value;

        if (!Uri.TryCreate(value, UriKind.Absolute, out var url))
            throw new InvalidOperationException($"{MetadataKey} must be an absolute URL of BookingHub.API; the build set \"{value}\".");

        // Request paths are relative ("api/v1/..."), so the base must end with a slash or its last segment is dropped.
        return url.AbsoluteUri.EndsWith('/') ? url : new Uri(url.AbsoluteUri + "/");
    }
}