using BookingHub.Mobile.Api.Contracts;

namespace BookingHub.Mobile.Api;

public interface IAuthApiClient
{
    Task<AuthenticatedResponse?> LoginAsync(string email, string password, CancellationToken cancellationToken);
    Task<AuthenticatedResponse?> RegisterAsync(string email, string password, CancellationToken cancellationToken);
    Task<AuthenticatedResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken);
    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken);
}