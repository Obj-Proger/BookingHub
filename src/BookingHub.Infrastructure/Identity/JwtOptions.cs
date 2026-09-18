namespace BookingHub.Infrastructure.Identity;

/// <remarks>
/// Bound from configuration ("Jwt" section) — <see cref="SigningKey"/> in particular must come
/// from a real secret store in any deployed environment (user-secrets locally, a proper secret
/// manager in production), never checked into source control.
/// </remarks>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public required string SigningKey { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public int ExpiryMinutes { get; init; } = 60;
    public int RefreshTokenExpiryDays { get; init; } = 30;
}