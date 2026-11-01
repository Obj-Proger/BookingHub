namespace BookingHub.API.RateLimiting;

/// <summary>
/// The named rate-limit policies endpoints opt into. Each counts requests per client IP address —
/// the browser's, forwarded by the Web app (see TrustedProxies) — over a sliding one-minute window.
/// Signed-in users are additionally limited per user across all endpoints.
/// </summary>
public static class RateLimitPolicies
{
    /// <summary>Anonymous reads: catalog, availability, a booking looked up by its token.</summary>
    public const string PublicRead = "public-read";

    /// <summary>Anonymous writes: booking, confirming, cancelling, joining the waitlist, reviewing.</summary>
    public const string PublicWrite = "public-write";

    /// <summary>Sign-in and registration — kept low against password guessing.</summary>
    public const string SignIn = "sign-in";

    /// <summary>Refreshing and revoking tokens, which a signed-in client does routinely.</summary>
    public const string Session = "session";
}