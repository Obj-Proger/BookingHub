namespace BookingHub.API.RateLimiting;

/// <summary>Requests allowed per minute; bound from the "RateLimiting" configuration section.</summary>
public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    /// <summary>Per client IP, <see cref="RateLimitPolicies.PublicRead"/>.</summary>
    public int PublicReadPermitLimit { get; init; } = 60;

    /// <summary>Per client IP, <see cref="RateLimitPolicies.PublicWrite"/>.</summary>
    public int PublicWritePermitLimit { get; init; } = 10;

    /// <summary>Per client IP, <see cref="RateLimitPolicies.SignIn"/>.</summary>
    public int SignInPermitLimit { get; init; } = 10;

    /// <summary>Per client IP, <see cref="RateLimitPolicies.Session"/>.</summary>
    public int SessionPermitLimit { get; init; } = 30;

    /// <summary>Per signed-in user, across every endpoint.</summary>
    public int AuthenticatedPermitLimit { get; init; } = 300;
}