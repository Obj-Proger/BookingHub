namespace BookingHub.Infrastructure.Identity;

public sealed record RefreshTokenValidationResult(bool Succeeded, Guid UserId, string? NewRawToken)
{
    public static readonly RefreshTokenValidationResult Failed = new(false, Guid.Empty, null);

    public static RefreshTokenValidationResult Success(Guid userId, string newRawToken) => new(true, userId, newRawToken);
}

public interface IRefreshTokenService
{
    Task<string> IssueAsync(Guid userId, CancellationToken cancellationToken);
    Task<RefreshTokenValidationResult> ValidateAndRotateAsync(string rawToken, CancellationToken cancellationToken);
    Task RevokeAsync(string rawToken, CancellationToken cancellationToken);
}