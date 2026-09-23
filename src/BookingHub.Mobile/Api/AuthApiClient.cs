using System.Net.Http.Json;
using BookingHub.Mobile.Api.Contracts;

namespace BookingHub.Mobile.Api;

/// <summary>
/// Built on the "AuthClient" named HttpClient (MauiProgram) — deliberately without
/// AuthTokenHandler attached. Every endpoint here is anonymous (no bearer token needed), and
/// RefreshAsync specifically must never go through the handler that itself calls this client,
/// or a failed refresh would try to refresh its own refresh call.
/// </summary>
internal sealed class AuthApiClient(HttpClient httpClient) : IAuthApiClient
{
    public async Task<AuthenticatedResponse?> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync("api/v1/auth/login", new LoginRequest(email, password), ApiJsonOptions.Default, cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<AuthenticatedResponse>(ApiJsonOptions.Default, cancellationToken)
            : null;
    }

    public async Task<AuthenticatedResponse?> RegisterAsync(string email, string password, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync("api/v1/auth/register", new RegisterRequest(email, password), cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<AuthenticatedResponse>(cancellationToken)
            : null;
    }

    public async Task<AuthenticatedResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync("api/v1/auth/refresh", new RefreshRequest(refreshToken), cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<AuthenticatedResponse>(cancellationToken)
            : null;
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken) =>
        await httpClient.PostAsJsonAsync("api/v1/auth/logout", new RefreshRequest(refreshToken), cancellationToken);
}