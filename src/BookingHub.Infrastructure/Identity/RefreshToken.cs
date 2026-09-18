namespace BookingHub.Infrastructure.Identity;

/// <summary>
/// Pure authentication bookkeeping, not a Domain concept — lives here, not in Domain, the same
/// way ApplicationUser does. Stores a SHA-256 hash of the raw token, never the token itself.
/// </summary>
internal sealed class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }

    private RefreshToken(Guid id, Guid userId, string tokenHash, DateTime expiresAtUtc, DateTime createdAtUtc)
    {
        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = createdAtUtc;
    }

    private RefreshToken()
    {
    }

    public static RefreshToken Create(Guid userId, string tokenHash, DateTime expiresAtUtc, DateTime utcNow) =>
        new(Guid.CreateVersion7(), userId, tokenHash, expiresAtUtc, utcNow);

    public bool IsActive(DateTime utcNow) => RevokedAtUtc is null && ExpiresAtUtc > utcNow;

    public void Revoke(DateTime utcNow) => RevokedAtUtc ??= utcNow;
}