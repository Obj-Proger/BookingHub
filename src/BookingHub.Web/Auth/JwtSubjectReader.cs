using System.Buffers.Text;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace BookingHub.Web.Auth;

/// <summary>
/// Reads the "sub" claim of a JWT the Web app has just received from its own API over the
/// API connection. The signature is deliberately not validated here — the token is only
/// inspected, never trusted for authorization decisions.
/// </summary>
internal static class JwtSubjectReader
{
    public static bool TryRead(string jwt, [NotNullWhen(true)] out string? subject)
    {
        subject = null;

        var parts = jwt.Split('.');
        if (parts.Length != 3)
            return false;

        try
        {
            using var payload = JsonDocument.Parse(Base64Url.DecodeFromChars(parts[1]));

            if (payload.RootElement.TryGetProperty("sub", out var claim) && claim.ValueKind == JsonValueKind.String)
                subject = claim.GetString();
        }
        catch (FormatException)
        {
            return false;
        }
        catch (JsonException)
        {
            return false;
        }

        return !string.IsNullOrEmpty(subject);
    }
}