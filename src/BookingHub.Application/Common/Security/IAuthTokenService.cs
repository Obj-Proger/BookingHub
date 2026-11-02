namespace BookingHub.Application.Common.Security;

/// <summary>A short-lived access token and the single-use refresh token that renews it.</summary>
public sealed record AuthTokens(string AccessToken, string RefreshToken);

/// <summary>Issues, renews and revokes a signed-in user's tokens.</summary>
public interface IAuthTokenService
{
    Task<AuthTokens> IssueAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>Exchanges a refresh token for a new pair; the old refresh token stops working.</summary>
    /// <returns>Null when the refresh token is unknown, expired or already used.</returns>
    Task<AuthTokens?> RefreshAsync(string refreshToken, CancellationToken cancellationToken);

    /// <summary>Signs a session out. An unknown or already revoked token is not an error.</summary>
    Task RevokeAsync(string refreshToken, CancellationToken cancellationToken);
}