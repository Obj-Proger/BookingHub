using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Api;

public enum LoginOutcome
{
    Success,
    InvalidCredentials,
    TooManyRequests,
    Unavailable
}

public sealed record LoginResult(LoginOutcome Outcome, AuthenticatedResponse? Tokens = null);

/// <summary>Anonymous calls to the API's /auth endpoints.</summary>
public interface IAuthApiClient
{
    Task<LoginResult> LoginAsync(string email, string password, CancellationToken cancellationToken);

    /// <summary>
    /// Returns null when the API rejects the refresh token. Transport failures and any other
    /// unsuccessful status throw <see cref="HttpRequestException"/>.
    /// </summary>
    Task<AuthenticatedResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken);

    /// <summary>Best-effort: transport failures and unsuccessful statuses are ignored.</summary>
    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken);
}