using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using BookingHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using BookingHub.Infrastructure.Identity;

namespace BookingHub.Infrastructure.Identity;

internal sealed class RefreshTokenService(ApplicationDbContext dbContext, TimeProvider timeProvider, IOptions<JwtOptions> jwtOptions)
    : IRefreshTokenService
{
    public async Task<string> IssueAsync(Guid userId, CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var rawToken = GenerateRawToken();

        dbContext.RefreshTokens.Add(RefreshToken.Create(
            userId, HashToken(rawToken), utcNow.AddDays(jwtOptions.Value.RefreshTokenExpiryDays), utcNow));
        await dbContext.SaveChangesAsync(cancellationToken);

        return rawToken;
    }

    public async Task<RefreshTokenValidationResult> ValidateAndRotateAsync(string rawToken, CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var tokenHash = HashToken(rawToken);

        var existing = await dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);
        if (existing is null)
            return RefreshTokenValidationResult.Failed;

        if (existing.RevokedAtUtc is not null)
        {
            // Reuse of an already-rotated token — likely theft. Revoke everything still active
            // for this user, not just this one, forcing every device to log in again.
            var activeTokens = await dbContext.RefreshTokens
                .Where(t => t.UserId == existing.UserId && t.RevokedAtUtc == null)
                .ToListAsync(cancellationToken);

            foreach (var token in activeTokens)
                token.Revoke(utcNow);

            await dbContext.SaveChangesAsync(cancellationToken);
            return RefreshTokenValidationResult.Failed;
        }

        if (!existing.IsActive(utcNow))
            return RefreshTokenValidationResult.Failed;

        existing.Revoke(utcNow);

        var newRawToken = GenerateRawToken();
        dbContext.RefreshTokens.Add(RefreshToken.Create(
            existing.UserId, HashToken(newRawToken), utcNow.AddDays(jwtOptions.Value.RefreshTokenExpiryDays), utcNow));

        await dbContext.SaveChangesAsync(cancellationToken);
        return RefreshTokenValidationResult.Success(existing.UserId, newRawToken);
    }

    public async Task RevokeAsync(string rawToken, CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var tokenHash = HashToken(rawToken);

        var existing = await dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);
        if (existing is null || existing.RevokedAtUtc is not null)
            return;

        existing.Revoke(utcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string GenerateRawToken() => Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));

    private static string HashToken(string rawToken) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
}