using System.Net;
using System.Net.Http.Json;
using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Api;

internal sealed class AuthApiClient(HttpClient httpClient) : IAuthApiClient
{
    public async Task<LoginResult> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsJsonAsync(
                "api/v1/auth/login", new LoginRequest(email, password), ApiJsonOptions.Default, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return new LoginResult(LoginOutcome.Unavailable);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new LoginResult(LoginOutcome.Unavailable);
        }

        using (response)
        {
            return response.StatusCode switch
            {
                HttpStatusCode.OK => await ReadTokensAsync(response, cancellationToken),
                HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized => new LoginResult(LoginOutcome.InvalidCredentials),
                HttpStatusCode.TooManyRequests => new LoginResult(LoginOutcome.TooManyRequests),
                _ => new LoginResult(LoginOutcome.Unavailable)
            };
        }
    }

    public async Task<AuthenticatedResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "api/v1/auth/refresh", new RefreshRequest(refreshToken), ApiJsonOptions.Default, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AuthenticatedResponse>(ApiJsonOptions.Default, cancellationToken);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.PostAsJsonAsync(
                "api/v1/auth/logout", new LogoutRequest(refreshToken), ApiJsonOptions.Default, cancellationToken);
        }
        catch (HttpRequestException)
        {
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
        }
    }

    private static async Task<LoginResult> ReadTokensAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var tokens = await response.Content.ReadFromJsonAsync<AuthenticatedResponse>(ApiJsonOptions.Default, cancellationToken);
        return tokens is null
            ? new LoginResult(LoginOutcome.Unavailable)
            : new LoginResult(LoginOutcome.Success, tokens);
    }
}