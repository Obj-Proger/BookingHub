using BookingHub.Application.Common.Security;

namespace BookingHub.Infrastructure.Identity;

/// <summary>A JWT access token plus a rotating refresh token stored (as a hash) in the database.</summary>
internal sealed class AuthTokenService(JwtTokenGenerator jwtTokenGenerator, RefreshTokenService refreshTokenService)
    : IAuthTokenService
{
    public async Task<AuthTokens> IssueAsync(Guid userId, CancellationToken cancellationToken)
    {
        var refreshToken = await refreshTokenService.IssueAsync(userId, cancellationToken);
        return new AuthTokens(jwtTokenGenerator.GenerateToken(userId), refreshToken);
    }

    // A deleted user's refresh tokens are deleted with it (cascade), so a token that rotates
    // successfully always belongs to an existing user.
    public async Task<AuthTokens?> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var rotation = await refreshTokenService.ValidateAndRotateAsync(refreshToken, cancellationToken);
        return rotation is null
            ? null
            : new AuthTokens(jwtTokenGenerator.GenerateToken(rotation.UserId), rotation.NewRawToken);
    }

    public Task RevokeAsync(string refreshToken, CancellationToken cancellationToken) =>
        refreshTokenService.RevokeAsync(refreshToken, cancellationToken);
}